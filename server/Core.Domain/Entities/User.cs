using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public class User
{
    public string Hash { get; set; } = null!;
    public string Salt { get; set; } = null!;
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime? Birthday { get; set; }
    public string Country { get; set; }
    public string Role { get; set; } = null!;
    public Weather Weather { get; set; }
    public UserSettings UserSettings { get; set; }
    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<SensorHistory> SensorHistories { get; set; } = new List<SensorHistory>();
}
