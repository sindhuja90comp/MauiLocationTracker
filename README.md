# MauiLocationTracker

[![Language: C#](https://img.shields.io/badge/language-C%23-blue.svg)]() 

A cross-platform .NET MAUI sample application for reading and tracking device location (foreground, with optional background support). Demonstrates requesting runtime permissions, obtaining current location, continuous tracking, and persisting location updates.

## Table of contents
- Features
- Screenshots
- Project structure
- Prerequisites
- Quick start
- Platform setup
  - Android
  - iOS / MacCatalyst
  - Windows
- Configuration
- Usage examples
  - Requesting permissions
  - Get current location
  - Continuous tracking
  - Persisting locations (SQLite example)
- Troubleshooting
- Contributing
- License
- Acknowledgements
- Contact

## Features
- Request and handle runtime location permissions
- Read current location (latitude, longitude, accuracy, timestamp)
- Continuous foreground tracking with configurable interval
- Example persistence to local SQLite database (optional)
- Platform-specific hooks for background tracking (Android foreground service, iOS background modes)

## Screenshots
screenshots in docs/screenshots/ and update paths below:
- Android main screen: docs/screenshots/android_main.png
- iOS map view: docs/screenshots/ios_map.png

![screenshot-placeholder](docs/screenshots/placeholder.png)

## Project structure
Repository root (selected files and directories):
- App.xaml
- App.xaml.cs
- MauiHeatMap.csproj
- MauiProgram.cs
- Data/
- Models/
- Platforms/
  - Android/
  - iOS/
- Services/
- ViewModels/
- Views/

Example tree (for copy/paste visualization):
```
/ (repository root)
├─ App.xaml
├─ App.xaml.cs
├─ MauiHeatMap.csproj
├─ MauiProgram.cs
├─ Data/
├─ Models/
├─ Platforms/
│  ├─ Android/
│  └─ iOS/
├─ Services/
├─ ViewModels/
└─ Views/
```

## Prerequisites
- .NET SDK matching the project's TargetFramework(s) (check MauiHeatMap.csproj for exact target, commonly net7.0 or net8.0)
- .NET MAUI workload installed
- Visual Studio 2022/2023 (Windows) or Visual Studio for Mac with MAUI support
- Android SDK and an emulator or device for Android testing
- Xcode and Apple device/simulator for iOS/macOS builds (macOS required for iOS)

## Quick start
1. Clone the repository:
   git clone https://github.com/sindhuja90comp/MauiLocationTracker.git
   cd MauiLocationTracker

2. Restore and build:
   dotnet restore
   dotnet build

3. Run (select appropriate target):
   - From Visual Studio: open the solution and choose the target platform (Android / iOS / Windows / MacCatalyst) and run.
   - From CLI (Android example):
     dotnet build -f:net8.0-android
     Use Visual Studio or dotnet run with appropriate parameters for device/emulator.

## Platform setup

### Android
- AndroidManifest.xml (Platforms/Android/AndroidManifest.xml) should include:
```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<!-- If background location is required on Android 10+ -->
<uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />
```
- Implement runtime permission requests in code.
- For persistent background tracking, implement a foreground service and add service declarations to the manifest. Follow Play Store policies for background location usage.

### iOS / MacCatalyst
- Info.plist (Platforms/iOS/Info.plist) should include usage descriptions:
  - NSLocationWhenInUseUsageDescription
  - NSLocationAlwaysAndWhenInUseUsageDescription (if requesting always permission)
- Enable Background Modes → Location updates in the app capabilities if background location updates are required.
- Follow App Store guidelines when requesting always-on location permission.

### Windows
- Ensure app has location capability configured when packaging or using platform-specific manifests.
- Confirm device location services are enabled at the OS level.

## Configuration
Central configuration options to expose in code:
- Desired accuracy (e.g., Best, High)
- Tracking interval (seconds)
- Minimum distance filter (meters)
- Persistence toggle (enable/disable saving to local DB)

Example constants:
```csharp
public static class TrackingConfig
{
    public const int IntervalSeconds = 10;
    public const double MinimumDistanceMeters = 5;
    public const bool PersistLocations = true;
}
```

## Usage examples

Requesting permissions:
```csharp
try
{
    var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
    if (status != PermissionStatus.Granted)
    {
        // Handle permission denied
    }
}
catch (Exception ex)
{
    // Handle exceptions
}
```

Get current location:
```csharp
var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
var location = await Geolocation.Default.GetLocationAsync(request);

if (location != null)
{
    var lat = location.Latitude;
    var lon = location.Longitude;
    var accuracy = location.Accuracy;
    var timestamp = location.Timestamp;
    // Use or display location
}
```

Continuous tracking (foreground):
```csharp
CancellationTokenSource cts = new();
var request = new GeolocationRequest(GeolocationAccuracy.Best);

async Task StartTrackingAsync()
{
    try
    {
        while (!cts.IsCancellationRequested)
        {
            var loc = await Geolocation.Default.GetLocationAsync(request, cts.Token);
            if (loc != null)
            {
                // Persist or process location
            }
            await Task.Delay(TimeSpan.FromSeconds(TrackingConfig.IntervalSeconds), cts.Token);
        }
    }
    catch (OperationCanceledException) { }
    catch (Exception ex)
    {
        // Handle errors
    }
}

void StopTracking() => cts.Cancel();
```

Persisting locations (SQLite-net-pcl example):
```csharp
public class LocationRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

// Saving
await db.InsertAsync(new LocationRecord {
    Latitude = loc.Latitude,
    Longitude = loc.Longitude,
    Accuracy = loc.Accuracy,
    Timestamp = loc.Timestamp
});
```

## Troubleshooting
- Permission denied: Confirm runtime permission is requested and that Info.plist / AndroidManifest include required keys/permissions.
- No location on emulator: Simulate location in the emulator (Android Studio AVD → Location; iOS Simulator → Debug → Location).
- Background tracking not working: Verify background capabilities and platform-specific requirements (iOS background mode enabled; Android foreground service and background permission).
- Build errors: Ensure installed .NET SDK and MAUI workloads match the project's TargetFramework(s) in MauiHeatMap.csproj.

## Contributing
Contributions are welcome. Recommended workflow:
- Fork the repository
- Create a branch for your changes
- Add tests where applicable
- Open a pull request with a clear description and testing steps

Include platform-specific testing notes and screenshots for UI changes.

## Acknowledgements
- .NET MAUI documentation — https://learn.microsoft.com/dotnet/maui
- CommunityToolkit.Maui and other community libraries commonly used with MAUI apps

## Contact
Open an issue in this repository for questions, feature requests, or bug reports.
