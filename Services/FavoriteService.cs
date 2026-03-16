using Eco_Meteo_Dashboard.Data;
using Eco_Meteo_Dashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Eco_Meteo_Dashboard.Services;

public interface IFavoriteService
{
    Task<List<FavoriteLocation>> GetAllAsync();
    Task<FavoriteLocation?> AddAsync(Location location);
    Task<bool> RemoveAsync(int id);
    Task<bool> ExistsAsync(double lat, double lon);
}

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;

    public FavoriteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FavoriteLocation>> GetAllAsync()
    {
        return await _context.FavoriteLocations
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<FavoriteLocation?> AddAsync(Location location)
    {
        if (await ExistsAsync(location.Latitude, location.Longitude))
            return null;

        var favorite = new FavoriteLocation
        {
            Name = location.Name,
            Country = location.Country,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            State = location.State,
            CreatedAt = DateTime.UtcNow
        };

        _context.FavoriteLocations.Add(favorite);
        await _context.SaveChangesAsync();
        return favorite;
    }

    public async Task<bool> RemoveAsync(int id)
    {
        var favorite = await _context.FavoriteLocations.FindAsync(id);
        if (favorite == null) return false;

        _context.FavoriteLocations.Remove(favorite);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(double lat, double lon)
    {
        return await _context.FavoriteLocations
            .AnyAsync(f => f.Latitude == lat && f.Longitude == lon);
    }
}
