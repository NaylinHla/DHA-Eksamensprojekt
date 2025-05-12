using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models.Dtos.MqttSubscriptionDto;

public class DeviceSensorDataDto
{
    [Required]
    [MinLength(1)]
    [JsonPropertyName("deviceId")]
    public required string DeviceId { get; set; }

    [Required]
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [Required]
    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    [Required]
    [JsonPropertyName("pressure")]
    public double AirPressure { get; set; }

    [Required]
    [JsonPropertyName("air_quality_analog")]
    public int AirQuality { get; set; }

    [Required]
    [JsonPropertyName("timestamp")]
    public DateTime Time { get; set; }
}