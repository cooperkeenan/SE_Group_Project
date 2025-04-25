namespace EnviroMonitorApp.Models;

public class AirQualityRecord
{
    public DateTime DateTime { get; set; }
    public double NitrogenDioxide { get; set; }
    public double SulphurDioxide { get; set; }
    public double PM25 { get; set; }
    public double PM10 { get; set; }
}
