namespace Eco_Meteo_Dashboard.Models;

public class ForecastData
{
    public DateTime Date { get; set; }
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Pop { get; set; }
}

public class HourlyForecast
{
    public List<ForecastData> Hours { get; set; } = new();
}

public class DailyForecast
{
    public List<ForecastData> Days { get; set; } = new();
}
