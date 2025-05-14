namespace Core.Domain.Entities;

public class ConditionAlertPlant
{
    public required Guid ConditionAlertPlantId { get; set; }
    public required Guid PlantId { get; set; }
    public required bool WaterNotify { get; set; }

    public Plant? Plant { get; set; }
    public ICollection<Alert>? Alerts { get; set; }
}