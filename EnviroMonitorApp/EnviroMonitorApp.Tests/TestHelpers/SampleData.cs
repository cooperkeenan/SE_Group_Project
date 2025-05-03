// EnviroMonitorApp.Tests/TestHelpers/SampleData.cs
using System;
using System.Collections.Generic;
using EnviroMonitorApp.Models;  

namespace EnviroMonitorApp.Tests.TestHelpers;

/// <summary>
/// Re-usable objects & JSON stubs for unit tests.
/// </summary>
public static class SampleData
{
    // ─────────────────────────────────────────────
    //  ──── Weather ────
    // ─────────────────────────────────────────────
    public static readonly IReadOnlyList<WeatherRecord> WeatherRecords = new[]
    {
        new WeatherRecord
        {
            Timestamp    = new DateTime(2025, 05, 01, 12, 00, 00, DateTimeKind.Utc),
            Temperature  = 21.3,
            Humidity     = 60,
            Pressure     = 1012
        },
        new WeatherRecord
        {
            Timestamp    = new DateTime(2025, 05, 01, 13, 00, 00, DateTimeKind.Utc),
            Temperature  = 22.0,
            Humidity     = 58,
            Pressure     = 1011
        }
    };

    /// <summary>
    /// Minimal JSON the real weather API would return for the “latest” endpoint.
    /// Useful with <c>FakeHttpHandler</c>.
    /// </summary>
    public const string LatestWeatherJson =
        """
        {
          "temperature": 21.3,
          "humidity": 60,
          "pressure": 1012
        }
        """;

    // ─────────────────────────────────────────────
    //  ──── Air Quality ────
    // ─────────────────────────────────────────────
    public static readonly AirQualityRecord GoodAir = new()
    {
        Timestamp   = new DateTime(2025, 05, 01, 12, 00, 00, DateTimeKind.Utc),
        Pm25        = 8.1,
        Pm10        = 12.4,
        No2         = 5.6,
        Category    = "Good"
    };

    public const string GoodAirJson =
        """
        {
          "pm25": 8.1,
          "pm10": 12.4,
          "no2": 5.6,
          "category": "Good"
        }
        """;

    // ─────────────────────────────────────────────
    //  ──── Water Quality ────
    // ─────────────────────────────────────────────
    public static readonly WaterQualityRecord SampleWater = new()
    {
        Timestamp     = new DateTime(2025, 05, 01, 12, 00, 00, DateTimeKind.Utc),
        PH            = 7.2,
        DissolvedO2   = 9.8,
        Turbidity     = 0.5
    };

    public const string SampleWaterJson =
        """
        {
          "ph": 7.2,
          "dissolved_o2": 9.8,
          "turbidity": 0.5
        }
        """;

    // ─────────────────────────────────────────────
    //  You can add helpers for quick clones
    // ─────────────────────────────────────────────
    public static WeatherRecord CloneWeather(double temp) =>
        new()
        {
            Timestamp   = DateTime.UtcNow,
            Temperature = temp,
            Humidity    = 50,
            Pressure    = 1010
        };
}
