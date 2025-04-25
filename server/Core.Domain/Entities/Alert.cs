using System;

namespace Core.Domain.Entities;

public class Alert
{
    public Guid AlertID { get; set; } = null!;
    public Guid AlertUserId { get; set; }
    public string AlertName { get; set; } = null!;
    public string AlertDesc { get; set; } = null!;
    public DateTime AlertTime { get; set; } = null!;
    public Guid? AlertPlant { get; set; } = null!;

    public User User { get; set; }
    public Plant Plant { get; set; }
}
