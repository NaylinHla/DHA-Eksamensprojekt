using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class ConditionAlertUserDeviceCreateDto
{
    [Required] [MaxLength(1000)] public string UserDeviceId { get; init; } = null!;

    [Required] [MaxLength(50)] public string SensorType { get; init; } = null!;

    [Required] [MaxLength(20)] public string Condition { get; init; } = null!;
}