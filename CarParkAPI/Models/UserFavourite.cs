namespace CarParkAPI.Models;

public class UserFavorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!; 
    public string CarParkNo { get; set; } = null!;
    public CarPark CarPark { get; set; } = null!;
}