using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;
using MauiHeatMap.Data;
using MauiHeatMap.Models;
using MauiHeatMap.Services;

namespace MauiHeatMap.Views;

public partial class MainPage : ContentPage, IDrawable
{
    private readonly AppDatabase _db;
    private readonly LocationTracker _tracker;
    private readonly List<LocationSample> _samples = new();

    // Heat circle visual params (tweak to taste)
    private const float CircleRadiusPx = 40f; // visual radius per sample
    private const float Alpha = 0.15f;        // per-sample opacity

    public MainPage(AppDatabase db, LocationTracker tracker)
    {
        InitializeComponent();
        _db = db;
        _tracker = tracker;
        HeatOverlay.Drawable = this;
        _tracker.OnSample += s =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _samples.Add(s);
                HeatOverlay.Invalidate();
            });
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Request permissions at first launch
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        // Load persisted points
        var all = await _db.GetAllAsync();
        _samples.Clear();
        _samples.AddRange(all);

        // Center map on last known or a default
        if (_samples.Count > 0)
        {
            var last = _samples.Last();
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(last.Latitude, last.Longitude),
                Distance.FromKilometers(1)));
        }
        else
        {
            // Default to a visible world span if nothing saved yet
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(37.7749, -122.4194), // SF
                Distance.FromKilometers(5)));
        }

        HeatOverlay.Invalidate();
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        StartBtn.IsEnabled = false;
        StopBtn.IsEnabled = true;
        // Sample every 10 seconds
        _ = _tracker.StartAsync(TimeSpan.FromSeconds(10));
    }

    private void OnStopClicked(object sender, EventArgs e)
    {
        _tracker.Stop();
        StartBtn.IsEnabled = true;
        StopBtn.IsEnabled = false;
    }

    private async void OnClearClicked(object sender, EventArgs e)
    {
        await _db.DeleteAllAsync();
        _samples.Clear();
        HeatOverlay.Invalidate();
    }

    /// <summary>
    /// Draws the heat map as semi-transparent radial circles at each sample.
    /// For assignments, this simple kernel is acceptable; you can upgrade to KDE/tiling later.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_samples.Count == 0) return;

        // Match overlay size to the map view
        var mapBounds = Map.Bounds;
        var mapRect = new RectF(0, 0, (float)Map.Width, (float)Map.Height);

        // Draw multiple passes for intensity
        foreach (var s in _samples)
        {
            // Convert geo -> screen point relative to Map
            var screen = GeoToPoint(s.Latitude, s.Longitude);
            if (screen is null) continue;

            canvas.SaveState();

            // Soft circle (draw several concentric circles for a crude radial falloff)
            for (int i = 0; i < 4; i++)
            {
                float r = CircleRadiusPx * (1f - i * 0.22f);
                float a = Alpha * (1f - i * 0.22f);

                canvas.FillColor = Colors.Red.WithAlpha(a);
                canvas.FillCircle((float)screen.Value.X, (float)screen.Value.Y, r);
            }

            canvas.RestoreState();
        }
    }

    private Point? GeoToPoint(double lat, double lng)
    {
        if (Map.VisibleRegion is null || Map.Width <= 0 || Map.Height <= 0)
            return null;

        var region = Map.VisibleRegion;
        var topLeft = new Location(region.Center.Latitude + region.LatitudeDegrees / 2,
                                   region.Center.Longitude - region.LongitudeDegrees / 2);

        var bottomRight = new Location(region.Center.Latitude - region.LatitudeDegrees / 2,
                                       region.Center.Longitude + region.LongitudeDegrees / 2);

        // Normalize
        double x = (lng - topLeft.Longitude) / (bottomRight.Longitude - topLeft.Longitude);
        double y = (topLeft.Latitude - lat) / (topLeft.Latitude - bottomRight.Latitude);

        // Convert to pixels
        return new Point(x * Map.Width, y * Map.Height);
    }
}
