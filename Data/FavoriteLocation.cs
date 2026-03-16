using System.ComponentModel.DataAnnotations;

namespace Eco_Meteo_Dashboard.Data;

public class FavoriteLocation
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Country { get; set; } = string.Empty;
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public string? State { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
