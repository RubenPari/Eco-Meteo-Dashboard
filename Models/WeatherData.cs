namespace Eco_Meteo_Dashboard.Models;

public class WeatherData
{
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int WindDirection { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Pressure { get; set; }
    public int Visibility { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
}
