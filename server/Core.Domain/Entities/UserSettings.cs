namespace Core.Domain.Entities;

public class UserSettings
{
    public Guid UserId { get; set; }
    public bool Celsius { get; set; }
    public bool DarkTheme { get; set; }
    public bool ConfirmDialog { get; set; }
    public bool SecretMode { get; set; }

    public User? User { get; set; }
}