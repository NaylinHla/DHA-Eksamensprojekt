namespace Application.Models.Dtos.MqttDtos.Response;

public class GetAllSensorHistoryByDeviceIdDto
{
    // Device information
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;

    // Array of sensor history readings
    public List<SensorHistoryDto> SensorHistoryRecords { get; set; } = new();
}

public class SensorHistoryDto
{
    // Sensor readings
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double AirPressure { get; set; }
    public int AirQuality { get; set; }

    // Timestamp of when the sensor reading was recorded
    public DateTime Time { get; set; }
}