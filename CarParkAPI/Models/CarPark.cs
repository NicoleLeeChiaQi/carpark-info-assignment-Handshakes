using System.ComponentModel.DataAnnotations;

namespace CarParkAPI.Models;

public class CarPark
{
    [Key] public string CarParkNo { get; set; } = null!;
    [Required] public string Address { get; set; } = null!;
    public double XCoord { get; set; }
    public double YCoord { get; set; }
    public int CarParkTypeId { get; set; }
    public CarParkType CarParkType { get; set; } = null!;
    public int ParkingSystemId { get; set; }
    public ParkingSystem ParkingSystem { get; set; } = null!;
    public int CarParkDecks { get; set; }
    public decimal GantryHeight { get; set; }
    public bool HasBasement { get; set; }
    public ParkingPolicy ParkingPolicy { get; set; } = null!;
}

public class CarParkType { public int Id { get; set; } public string Name { get; set; } = null!; }
public class ParkingSystem { public int Id { get; set; } public string Name { get; set; } = null!; }

public class ParkingPolicy
{
    public int Id { get; set; }
    [Required] public string CarParkNo { get; set; } = null!;
    public CarPark CarPark { get; set; } = null!;
    public string ShortTermParking { get; set; } = null!;
    public string FreeParking { get; set; } = null!;
    public bool OffersNightParking { get; set; }
}

public class UserFavorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!; 
    public string CarParkNo { get; set; } = null!;
    public CarPark CarPark { get; set; } = null!;
}