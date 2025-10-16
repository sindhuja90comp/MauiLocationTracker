using SQLite;
using MauiHeatMap.Models;

namespace MauiHeatMap.Data;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _db;

    public AppDatabase(string path)
    {
        _db = new SQLiteAsyncConnection(path);
        _ = _db.CreateTableAsync<LocationSample>();
    }

    public Task<int> InsertAsync(LocationSample sample) => _db.InsertAsync(sample);

    public Task<List<LocationSample>> GetAllAsync() =>
        _db.Table<LocationSample>()
           .OrderBy(s => s.Timestamp)
           .ToListAsync();

    public Task<int> DeleteAllAsync() => _db.DeleteAllAsync<LocationSample>();
}
