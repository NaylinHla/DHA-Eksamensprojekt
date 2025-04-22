namespace Core.Domain.Entities;

public class Devicelog
{
    public string Deviceid { get; set; } = null!;

    public decimal Value { get; set; }

    public string Id { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}