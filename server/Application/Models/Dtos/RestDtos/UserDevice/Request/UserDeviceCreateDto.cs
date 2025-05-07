using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.UserDevice.Request;

public sealed class UserDeviceCreateDto
{
    [Required] [MaxLength(50)]
    public string DeviceName { get; init; } = null!;
    [MaxLength(500)]
    public string? DeviceDescription { get; init; }
    
    public required DateTime? Created { get; init; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string? WaitTime { get; init; }
}
