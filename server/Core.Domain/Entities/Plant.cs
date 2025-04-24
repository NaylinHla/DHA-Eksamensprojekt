using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public class Plant
{
    public Guid PlantID { get; set; }
    public DateTime? Planted { get; set; }
    public string PlantName { get; set; }
    public string PlantType { get; set; }
    public string PlantNotes { get; set; }
    public DateTime? LastWatered { get; set; }
    public int? WaterEvery { get; set; }
    public bool IsDead { get; set; }

    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
