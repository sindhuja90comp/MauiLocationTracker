using SQLite;

namespace MauiHeatMap.Models;

public class LocationSample
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? AccuracyMeters { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
