namespace Application.Models.Dtos.RestDtos;

public class ConditionAlertPlantResponseDto
{
    public required Guid ConditionAlertPlantId { get; set; }
    public required Guid PlantId { get; set; }
    public required bool WaterNotify { get; set; }
}