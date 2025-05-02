namespace Core.Domain.Entities;

public class Alert
{
    public required Guid AlertId { get; set; }
    public required Guid AlertUserId { get; set; }
    public required string AlertName { get; set; }
    public required string AlertDesc { get; set; }
    public required DateTime AlertTime { get; set; }
    public Guid? AlertPlant { get; set; }

    public User User { get; set; }
    public Plant Plant { get; set; }
}
