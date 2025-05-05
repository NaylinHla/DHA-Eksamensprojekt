using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public class Plant
{
    public required Guid PlantId { get; set; }
    public DateTime? Planted { get; set; }
    public string PlantName { get; set; } = null!;
    public string PlantType { get; set; } = null!;
    public string PlantNotes { get; set; } = null!;
    public DateTime? LastWatered { get; set; }
    public int? WaterEvery { get; set; }
    public bool IsDead { get; set; }

    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
