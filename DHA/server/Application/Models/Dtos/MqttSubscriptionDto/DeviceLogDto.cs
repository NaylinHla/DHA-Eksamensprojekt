using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.MqttSubscriptionDto;

public class DeviceLogDto
{
    [Required] [MinLength(1)] public string Unit { get; set; }
    [Required] [MinLength(1)] public int Value { get; set; }
    [Required] [MinLength(1)] public string DeviceId { get; set; }
}