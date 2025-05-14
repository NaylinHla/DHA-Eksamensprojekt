namespace Core.Domain.Entities;

public class ConditionAlertUserDevice
{
    public required Guid ConditionAlertUserDeviceId { get; set; }
    public required Guid UserDeviceId { get; set; }
    public required string SensorType { get; set; }
    public required string Condition { get; set; }

    public UserDevice? UserDevice { get; set; }
    public ICollection<Alert>? Alerts { get; set; }
}