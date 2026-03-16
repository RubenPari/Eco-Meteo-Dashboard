using Microsoft.EntityFrameworkCore;

namespace Eco_Meteo_Dashboard.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<FavoriteLocation> FavoriteLocations { get; set; }
}
