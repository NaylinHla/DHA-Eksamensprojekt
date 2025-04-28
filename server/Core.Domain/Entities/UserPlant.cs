using System;

namespace Core.Domain.Entities;

public class UserPlant
{
    public required Guid UserID { get; set; }
    public required Guid PlantID { get; set; }

    public User User { get; set; }
    public Plant Plant { get; set; }
}
