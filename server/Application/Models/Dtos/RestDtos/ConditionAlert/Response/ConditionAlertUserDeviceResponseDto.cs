namespace Application.Models.Dtos.RestDtos;

public class ConditionAlertUserDeviceResponseDto
{
    public required Guid ConditionAlertUserDeviceId { get; set; }
    public required Guid UserDeviceId { get; set; }
    public required string SensorType { get; set; }
    public required string Condition { get; set; }
}