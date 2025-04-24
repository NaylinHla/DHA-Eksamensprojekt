using System;

namespace Core.Domain.Entities;

public class UserPlant
{
    public Guid UserID { get; set; }
    public Guid PlantID { get; set; }

    public User User { get; set; }
    public Plant Plant { get; set; }
}
