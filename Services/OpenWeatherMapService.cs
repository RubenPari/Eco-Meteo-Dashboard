using Eco_Meteo_Dashboard.Models;

namespace Eco_Meteo_Dashboard.Services;

public interface IOpenWeatherMapService
{
    Task<WeatherData?> GetCurrentWeatherAsync(double lat, double lon);
    Task<HourlyForecast?> GetForecastAsync(double lat, double lon);
}

public class OpenWeatherMapService : IOpenWeatherMapService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenWeatherMapService> _logger;

    public OpenWeatherMapService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenWeatherMapService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string ApiKey => _configuration["ApiKeys:OpenWeatherMap"] ?? "";

    public async Task<WeatherData?> GetCurrentWeatherAsync(double lat, double lon)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<OpenWeatherResponse>(
                $"data/2.5/weather?lat={lat}&lon={lon}&appid={ApiKey}&units=metric");

            if (response == null) return null;

            return new WeatherData
            {
                Temperature = response.Main.Temp,
                FeelsLike = response.Main.FeelsLike,
                Humidity = response.Main.Humidity,
                WindSpeed = response.Wind.Speed,
                WindDirection = response.Wind.Deg,
                Description = response.Weather.FirstOrDefault()?.Description ?? "",
                Icon = response.Weather.FirstOrDefault()?.Icon ?? "",
                Pressure = response.Main.Pressure,
                Visibility = response.Visibility,
                TempMin = response.Main.TempMin,
                TempMax = response.Main.TempMax,
                Sunrise = DateTimeOffset.FromUnixTimeSeconds(response.Sys.Sunrise).DateTime,
                Sunset = DateTimeOffset.FromUnixTimeSeconds(response.Sys.Sunset).DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            return null;
        }
    }

    public async Task<HourlyForecast?> GetForecastAsync(double lat, double lon)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ForecastResponse>(
                $"data/2.5/forecast?lat={lat}&lon={lon}&appid={ApiKey}&units=metric");

            if (response == null) return null;

            var forecasts = response.List.Select(item => new ForecastData
            {
                Date = DateTime.Parse(item.DtTxt),
                Temperature = item.Main.Temp,
                FeelsLike = item.Main.FeelsLike,
                Humidity = item.Main.Humidity,
                WindSpeed = item.Wind.Speed,
                Description = item.Weather.FirstOrDefault()?.Description ?? "",
                Icon = item.Weather.FirstOrDefault()?.Icon ?? "",
                Pop = (int)(item.Pop * 100)
            }).ToList();

            return new HourlyForecast { Hours = forecasts };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching forecast data");
            return null;
        }
    }
}

internal class OpenWeatherResponse
{
    public MainData Main { get; set; } = new();
    public WeatherInfo[] Weather { get; set; } = [];
    public WindData Wind { get; set; } = new();
    public SysData Sys { get; set; } = new();
    public int Visibility { get; set; }
}

internal class MainData
{
    public double Temp { get; set; }
    public double FeelsLike { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public int Humidity { get; set; }
    public int Pressure { get; set; }
}

internal class WeatherInfo
{
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
}

internal class WindData
{
    public double Speed { get; set; }
    public int Deg { get; set; }
}

internal class SysData
{
    public long Sunrise { get; set; }
    public long Sunset { get; set; }
}

internal class ForecastResponse
{
    public List<ForecastItem> List { get; set; } = [];
}

internal class ForecastItem
{
    public string DtTxt { get; set; } = "";
    public MainData Main { get; set; } = new();
    public WeatherInfo[] Weather { get; set; } = [];
    public WindData Wind { get; set; } = new();
    public double Pop { get; set; }
}
