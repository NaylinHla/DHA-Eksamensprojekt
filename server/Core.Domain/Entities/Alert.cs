namespace Core.Domain.Entities;

public class Alert
{
    public required Guid AlertId { get; set; }
    public required Guid AlertUserId { get; set; }
    public required string AlertName { get; set; }
    public required string AlertDesc { get; set; }
    public required DateTime AlertTime { get; set; }
    public Guid? AlertPlantConditionId { get; set; }
    public Guid? AlertDeviceConditionId { get; set; }

    public User? User { get; set; }
    public ConditionAlertPlant? ConditionAlertPlant { get; set; }
    public ConditionAlertUserDevice? ConditionAlertUserDevice { get; set; }
}