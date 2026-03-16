using Eco_Meteo_Dashboard.Models;

namespace Eco_Meteo_Dashboard.Services;

public interface IAirQualityService
{
    Task<AirQualityData?> GetCurrentAQIAsync(double lat, double lon);
}

public class AirQualityService : IAirQualityService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AirQualityService> _logger;

    public AirQualityService(HttpClient httpClient, IConfiguration configuration, ILogger<AirQualityService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string ApiKey => _configuration["ApiKeys:IQAir"] ?? "";

    public async Task<AirQualityData?> GetCurrentAQIAsync(double lat, double lon)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IQAirResponse>(
                $"api/v3/air_quality/nearest_city?lat={lat}&lon={lon}&key={ApiKey}");

            if (response?.Data == null) return null;

            var data = response.Data;
            return new AirQualityData
            {
                Aqi = data.Current?.Pollution?.Aqi ?? 0,
                Category = GetAqiCategory(data.Current?.Pollution?.Aqi ?? 0),
                PM25 = data.Current?.Pollution?.Pm25 ?? 0,
                PM10 = data.Current?.Pollution?.Pm10 ?? 0,
                NO2 = data.Current?.Pollution?.No2 ?? 0,
                O3 = data.Current?.Pollution?.O3 ?? 0,
                SO2 = data.Current?.Pollution?.So2 ?? 0,
                CO = data.Current?.Pollution?.Co ?? 0,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching AQI data");
            return null;
        }
    }

    private static string GetAqiCategory(int aqi)
    {
        return aqi switch
        {
            <= 50 => "Good",
            <= 100 => "Moderate",
            <= 150 => "Unhealthy for Sensitive Groups",
            <= 200 => "Unhealthy",
            <= 300 => "Very Unhealthy",
            _ => "Hazardous"
        };
    }
}

internal class IQAirResponse
{
    public IQAirData? Data { get; set; }
}

internal class IQAirData
{
    public CurrentPollution? Current { get; set; }
}

internal class CurrentPollution
{
    public Pollution? Pollution { get; set; }
}

internal class Pollution
{
    public int? Aqi { get; set; }
    public double? Pm25 { get; set; }
    public double? Pm10 { get; set; }
    public double? No2 { get; set; }
    public double? O3 { get; set; }
    public double? So2 { get; set; }
    public double? Co { get; set; }
}
