using System;

namespace Core.Domain.Entities;

public class SensorHistory
{
    public required Guid HistoryId { get; set; }
    public string DeviceId { get; set; } = null!;
    public int Temperature { get; set; }
    public int Humidity { get; set; }
    public int AirPressure { get; set; }
    public int AirQuality { get; set; }
    public DateTime Time { get; set; }

    public required User User { get; set; }
}
