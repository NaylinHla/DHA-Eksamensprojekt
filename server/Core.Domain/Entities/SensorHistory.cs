using System;

namespace Core.Domain.Entities;

public class SensorHistory
{
    public required Guid SensorHistoryId { get; set; }
    public Guid DeviceId { get; set; }
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public double AirPressure { get; set; }
    public int AirQuality { get; set; }
    public DateTime Time { get; set; }
    
    public UserDevice? UserDevice { get; set; }
}
