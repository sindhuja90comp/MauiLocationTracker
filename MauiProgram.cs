using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using MauiHeatMap.Data;
using MauiHeatMap.Services;
using MauiHeatMap.Views;

namespace MauiHeatMap;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // SQLite path
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
        builder.Services.AddSingleton(new AppDatabase(dbPath));
        builder.Services.AddSingleton<LocationTracker>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
