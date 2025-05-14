using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public sealed class ConditionAlertUserDeviceEditDto
{
    [MaxLength(50)] public string SensorType { get; init; } = null!;

    [MaxLength(20)] public string Condition { get; init; } = null!;
}