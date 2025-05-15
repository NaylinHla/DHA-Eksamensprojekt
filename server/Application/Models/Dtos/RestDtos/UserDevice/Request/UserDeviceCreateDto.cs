using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.UserDevice.Request;

public sealed class UserDeviceCreateDto
{
    [Required]
    [MaxLength(50)]
    [MinLength(2)]
    public string DeviceName { get; set; } = null!;

    [MaxLength(500)] public string? DeviceDescription { get; set; }

    public required DateTime? Created { get; set; } = DateTime.UtcNow;

    [Range(10, int.MaxValue)] public string? WaitTime { get; set; }
}