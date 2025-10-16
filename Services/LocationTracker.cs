using MauiHeatMap.Data;
using MauiHeatMap.Models;

namespace MauiHeatMap.Services;

/// <summary>
/// Polls the device location periodically while the app is in foreground.
/// Persists samples to SQLite.
/// </summary>
public class LocationTracker
{
    private readonly AppDatabase _db;
    private CancellationTokenSource? _cts;

    public event Action<LocationSample>? OnSample;

    public LocationTracker(AppDatabase db) => _db = db;

    public bool IsRunning => _cts is not null;

    public async Task StartAsync(TimeSpan interval)
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var loc = await Geolocation.GetLocationAsync(req, _cts.Token);

                if (loc is not null)
                {
                    var sample = new LocationSample
                    {
                        Latitude = loc.Latitude,
                        Longitude = loc.Longitude,
                        AccuracyMeters = loc.Accuracy,
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    await _db.InsertAsync(sample);
                    OnSample?.Invoke(sample);
                }
            }
            catch (Exception)
            {
                // Swallow transient errors (permissions, GPS off, timeout).
            }

            await Task.Delay(interval, _cts.Token);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
    }
}
