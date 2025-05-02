using System;

namespace Core.Domain.Entities;

public class UserPlant
{
    public required Guid UserId { get; set; }
    public required Guid PlantId { get; set; }

    public User User { get; set; } = null!;
    public Plant Plant { get; set; } = null!;
}
