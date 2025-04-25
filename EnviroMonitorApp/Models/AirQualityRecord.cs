using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EnviroMonitorApp.Models;

[Table("air_quality_records")]
[PrimaryKey(nameof(DateTime))]
public class AirQualityRecord
{
    [Column("datetime")]
    public DateTime DateTime { get; set; }
    [Column("nitrogen_dioxide")]
    public double NitrogenDioxide { get; set; }
    [Column("sulphur_dioxide")]
    public double SulphurDioxide { get; set; }
    [Column("pm25")]
    public double PM25 { get; set; }
    [Column("pm10")]
    public double PM10 { get; set; }
}
