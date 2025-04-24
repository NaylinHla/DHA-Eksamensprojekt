namespace Core.Domain.Entities;

public class User
{
    public string Id { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string Role { get; set; } = null!;
}