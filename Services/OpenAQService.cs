using System.Text.Json;
using System.Text.Json.Serialization;
using Eco_Meteo_Dashboard.Models;

namespace Eco_Meteo_Dashboard.Services;

public interface IAirQualityService
{
    Task<AirQualityData?> GetCurrentAQIAsync(double lat, double lon);
}

public class OpenAQService : IAirQualityService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAQService> _logger;

    private const int SearchRadiusMeters = 25000;

    public OpenAQService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAQService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string ApiKey => _configuration["ApiKeys:OpenAQ"] ?? "";

    public async Task<AirQualityData?> GetCurrentAQIAsync(double lat, double lon)
    {
        try
        {
            // Step 1: Find nearest location within radius
            var location = await FindNearestLocationAsync(lat, lon);
            if (location == null)
            {
                _logger.LogWarning("No OpenAQ monitoring station found within {Radius}m of ({Lat}, {Lon})", 
                    SearchRadiusMeters, lat, lon);
                return null;
            }

            // Step 2: Get latest measurements for the location
            var measurements = await GetLatestMeasurementsAsync(location.Id);
            if (measurements == null)
            {
                return null;
            }

            // Step 3: Calculate AQI from PM2.5 (primary pollutant)
            var pm25Value = measurements.GetValueOrDefault("pm25", 0);
            var aqi = CalculateAqiFromPm25(pm25Value);

            return new AirQualityData
            {
                Aqi = aqi,
                Category = GetAqiCategory(aqi),
                PM25 = pm25Value,
                PM10 = measurements.GetValueOrDefault("pm10", 0),
                NO2 = measurements.GetValueOrDefault("no2", 0),
                O3 = measurements.GetValueOrDefault("o3", 0),
                SO2 = measurements.GetValueOrDefault("so2", 0),
                CO = measurements.GetValueOrDefault("co", 0),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching AQI data from OpenAQ for ({Lat}, {Lon})", lat, lon);
            return null;
        }
    }

    private async Task<OpenAQLocation?> FindNearestLocationAsync(double lat, double lon)
    {
        var url = $"v3/locations?coordinates={lat},{lon}&radius={SearchRadiusMeters}&limit=1&order_by=distance";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(ApiKey))
        {
            request.Headers.Add("x-api-key", ApiKey);
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var locationsResponse = JsonSerializer.Deserialize<OpenAQLocationsResponse>(content, _jsonOptions);

        return locationsResponse?.Results?.FirstOrDefault();
    }

    private async Task<Dictionary<string, double>?> GetLatestMeasurementsAsync(string locationId)
    {
        var url = $"v3/locations/{locationId}/latest";
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(ApiKey))
        {
            request.Headers.Add("x-api-key", ApiKey);
        }

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get latest measurements for location {LocationId}: {StatusCode}", 
                locationId, response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var latestResponse = JsonSerializer.Deserialize<OpenAQLatestResponse>(content, _jsonOptions);

        if (latestResponse?.Measurements == null || latestResponse.Measurements.Count == 0)
        {
            return null;
        }

        // Get the most recent measurement for each parameter
        var result = new Dictionary<string, double>();
        
        // Group by parameter and get the most recent
        var grouped = latestResponse.Measurements
            .GroupBy(m => m.Parameter.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.LastUpdated).First());

        foreach (var group in grouped)
        {
            var measurement = group.Value;
            if (measurement.Value.HasValue)
            {
                // Convert from ppb to µg/m³ where needed
                var convertedValue = ConvertToMicrogramsPerCubicMeter(
                    measurement.Parameter.ToLowerInvariant(), 
                    measurement.Value.Value, 
                    measurement.Unit?.ToLowerInvariant() ?? ""
                );
                result[group.Key] = convertedValue;
            }
        }

        return result;
    }

    private double ConvertToMicrogramsPerCubicMeter(string parameter, double value, string unit)
    {
        // OpenAQ typically returns µg/m³ for particulate matter
        // For gases, it may return ppm or ppb
        if (unit == "ppm")
        {
            // Convert ppm to µg/m³ (at standard conditions)
            // Molecular weights: NO2=46, O3=48, SO2=64, CO=28
            return parameter switch
            {
                "no2" => value * 46.0 / 24.45,  // 24.45 L/mol at 25°C
                "o3" => value * 48.0 / 24.45,
                "so2" => value * 64.0 / 24.45,
                "co" => value * 28.0 / 24.45,
                _ => value
            };
        }
        if (unit == "ppb")
        {
            return parameter switch
            {
                "no2" => value * 46.0 / 1000.0,
                "o3" => value * 48.0 / 1000.0,
                "so2" => value * 64.0 / 1000.0,
                "co" => value * 28.0 / 1000.0,
                _ => value / 1000.0
            };
        }
        
        // Already in µg/m³ or unknown
        return value;
    }

    /// <summary>
    /// Calculate AQI from PM2.5 concentration (µg/m³) using EPA breakpoints
    /// </summary>
    private static int CalculateAqiFromPm25(double pm25)
    {
        // EPA PM2.5 AQI breakpoints
        return pm25 switch
        {
            <= 12.0 => (int)Math.Round((50.0 / 12.0) * pm25),
            <= 35.4 => (int)Math.Round(50 + (50.0 / 23.4) * (pm25 - 12.0)),
            <= 55.4 => (int)Math.Round(100 + (50.0 / 20.0) * (pm25 - 35.4)),
            <= 150.4 => (int)Math.Round(150 + (50.0 / 95.0) * (pm25 - 55.4)),
            <= 250.4 => (int)Math.Round(200 + (100.0 / 100.0) * (pm25 - 150.4)),
            _ => (int)Math.Round(300 + (100.0 / 100.0) * (pm25 - 250.4))
        };
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

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

// OpenAQ API Response DTOs

internal class OpenAQLocationsResponse
{
    [JsonPropertyName("results")]
    public List<OpenAQLocation>? Results { get; set; }
}

internal class OpenAQLocation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("coordinates")]
    public OpenAQCoordinates? Coordinates { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string? City { get; set; }
}

internal class OpenAQCoordinates
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

internal class OpenAQLatestResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("measurements")]
    public List<OpenAQMeasurement>? Measurements { get; set; }
}

internal class OpenAQMeasurement
{
    [JsonPropertyName("parameter")]
    public string Parameter { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
