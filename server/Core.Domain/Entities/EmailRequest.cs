namespace Core.Domain.Entities;

public class EmailRequest
{
    public string Subject { get; set; } = null!;
    public string Message { get; set; } = null!;
}