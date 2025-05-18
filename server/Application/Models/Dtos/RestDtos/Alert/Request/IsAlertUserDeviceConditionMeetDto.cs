using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos;

public class IsAlertUserDeviceConditionMeetDto
{
    [Required]
    [MaxLength(1000)]
    public string UserDeviceId { get; init; } = null!;

    public double? Temperature { get; init; }
    public double? Humidity { get; init; }
    public double? AirPressure { get; init; }
    public int? AirQuality { get; init; }

    public DateTime Time { get; init; } = DateTime.UtcNow;
}