using System.ComponentModel.DataAnnotations;

namespace CarParkAPI.Models;

public class ParkingPolicy
{
    public int Id { get; set; }
    
    [Required]
    public string CarParkNo { get; set; } = null!;
    public CarPark CarPark { get; set; } = null!;
    
    public string ShortTermParking { get; set; } = null!;
    public string FreeParking { get; set; } = null!;
    public bool OffersNightParking { get; set; }
}