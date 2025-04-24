using System;

namespace Core.Domain.Entities;

public class Alert
{
    public Guid AlertID { get; set; }
    public Guid AlertUserId { get; set; }
    public string AlertName { get; set; }
    public string AlertDesc { get; set; }
    public DateTime AlertTime { get; set; }
    public Guid? AlertPlant { get; set; }

    public User User { get; set; }
    public Plant Plant { get; set; }
}
