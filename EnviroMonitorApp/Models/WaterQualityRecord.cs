using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnviroMonitorApp.Models;

[Table("water_quality_records")]
[PrimaryKey(nameof(DateTime))]
public class WaterQualityRecord
{
    [Column("record_datetime")]
    public DateTime DateTime { get; set; }
    [Column("nitrate")]
    public double Nitrate { get; set; }
    [Column("nitrite")]
    public double Nitrite { get; set; }
    [Column("phosphate")]
    public double Phosphate { get; set; }
    [Column("ec")]
    public double EC { get; set; } // E. coli or similar measurement
}