using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public class Plant
{
    public Guid PlantID { get; set; } != null
    public DateTime? Planted { get; set; }
    public string PlantName { get; set; } != null
    public string PlantType { get; set; } != null
    public string PlantNotes { get; set; }
    public DateTime? LastWatered { get; set; }
    public int? WaterEvery { get; set; } != null
    public bool IsDead { get; set; } != null

    public ICollection<UserPlant> UserPlants { get; set; } = new List<UserPlant>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
