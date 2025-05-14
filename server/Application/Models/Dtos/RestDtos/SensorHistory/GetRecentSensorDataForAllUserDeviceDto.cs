namespace Application.Models.Dtos.RestDtos.SensorHistory;

public class GetRecentSensorDataForAllUserDeviceDto
{
    // Array of sensor history readings
    public List<SensorHistoryWithDeviceDto> SensorHistoryWithDeviceRecords { get; set; } = new();
}

public class SensorHistoryWithDeviceDto
{
    // Device information
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceDesc { get; set; } = null!;
    public string DeviceWaitTime { get; set; } = null!;
    public DateTime DeviceCreateDateTime { get; set; }

    // Sensor readings
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double AirPressure { get; set; }
    public int AirQuality { get; set; }

    // Timestamp of when the sensor reading was recorded
    public DateTime Time { get; set; }
}