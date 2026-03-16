using Eco_Meteo_Dashboard.Models;

namespace Eco_Meteo_Dashboard.Services;

public interface IGeocodingService
{
    Task<List<Location>> SearchCitiesAsync(string query);
}

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeocodingService> _logger;

    public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Location>> SearchCitiesAsync(string query)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<GeocodingResult>>(
                $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=10&addressdetails=1");

            if (response == null) return [];

            return response.Select(r => new Location
            {
                Name = r.Name,
                Country = r.Address?.Country ?? "",
                Latitude = double.TryParse(r.Lat, out var lat) ? lat : 0,
                Longitude = double.TryParse(r.Lon, out var lon) ? lon : 0,
                State = r.Address?.State
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching geocoding data");
            return [];
        }
    }
}

internal class GeocodingResult
{
    public string Name { get; set; } = "";
    public string Lat { get; set; } = "";
    public string Lon { get; set; } = "";
    public AddressInfo? Address { get; set; }
}

internal class AddressInfo
{
    public string? Country { get; set; }
    public string? State { get; set; }
}
