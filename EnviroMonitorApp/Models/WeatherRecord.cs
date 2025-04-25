using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnviroMonitorApp.Models;

[Table("weather_records")]
[PrimaryKey(nameof(DateTime))]
public class WeatherRecord
{
    [Column("record_datetime")]
    public DateTime DateTime { get; set; }

    [Column("temperature")]
    public float Temperature { get; set; }

    [Column("relative_humidity")]
    public int RelativeHumidity { get; set; }

    [Column("wind_speed")]
    public float WindSpeed { get; set; }

    [Column("wind_direction")]
    public int WindDirection { get; set; }
}