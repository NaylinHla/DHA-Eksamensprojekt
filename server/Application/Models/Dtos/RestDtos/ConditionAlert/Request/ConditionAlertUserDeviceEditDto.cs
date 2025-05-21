using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public sealed class ConditionAlertUserDeviceEditDto
{
    [Required] [MaxLength(1000)] public string ConditionAlertUserDeviceId { get; init; } = null!;
    
    [Required] [MaxLength(1000)] public string UserDeviceId { get; init; } = null!;
    
    [MaxLength(50)]
    [RegularExpression("^(Temperature|Humidity|AirPressure|AirQuality)$", ErrorMessage = "SensorType must be one of the following: Temperature, Humidity, AirPressure, AirQuality.")]
    public string SensorType { get; init; } = null!;
    
    [MaxLength(20)]
    [RegularExpression(@"^(<=|>=)\d+(\.\d+)?$", ErrorMessage = "Condition must be in the format '<=Number' or '>=Number' where Number can be an integer or decimal.")]
    public string Condition { get; init; } = null!;
}