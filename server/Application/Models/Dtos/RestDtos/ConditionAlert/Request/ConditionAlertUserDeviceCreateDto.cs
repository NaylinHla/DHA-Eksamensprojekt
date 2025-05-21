using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class ConditionAlertUserDeviceCreateDto
{
    public string UserDeviceId { get; init; } = null!;

    public string SensorType { get; init; } = null!;
    
    public string Condition { get; init; } = null!;
}