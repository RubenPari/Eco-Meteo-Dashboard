namespace Eco_Meteo_Dashboard.Models;

public class AirQualityData
{
    public int Aqi { get; set; }
    public string Category { get; set; } = string.Empty;
    public double PM25 { get; set; }
    public double PM10 { get; set; }
    public double NO2 { get; set; }
    public double O3 { get; set; }
    public double SO2 { get; set; }
    public double CO { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Pollutants
{
    public double PM25 { get; set; }
    public double PM10 { get; set; }
    public double NO2 { get; set; }
    public double O3 { get; set; }
    public double SO2 { get; set; }
    public double CO { get; set; }
}
