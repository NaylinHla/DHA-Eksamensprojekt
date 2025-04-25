using System;

namespace Core.Domain.Entities;

public class UserPlant
{
    public Guid UserID { get; set; } != null
    public Guid PlantID { get; set; } != null

    public User User { get; set; }
    public Plant Plant { get; set; }
}
