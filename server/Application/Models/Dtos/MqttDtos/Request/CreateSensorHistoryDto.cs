namespace Application.Models.Dtos.MqttDtos.Request;

public class CreateSensorHistoryDto
{
    // ID of the device associated with the sensor history
    public required Guid DeviceId { get; set; }

    // Sensor readings
    public int Temperature { get; set; }
    public int Humidity { get; set; }
    public int AirPressure { get; set; }
    public int AirQuality { get; set; }

    // Timestamp of when the sensor reading was recorded
    public DateTime Time { get; set; }
}