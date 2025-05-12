namespace Core.Domain.Entities;

public class Weather
{
    public Guid UserId { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }

    public User? User { get; set; }
}