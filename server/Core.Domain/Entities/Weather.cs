using System;

namespace Core.Domain.Entities;

public class Weather
{
    public Guid UserId { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    public User User { get; set; }
}
