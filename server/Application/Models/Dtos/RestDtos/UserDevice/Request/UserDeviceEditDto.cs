using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.UserDevice.Request;

public class UserDeviceEditDto
{
    [MaxLength(50)]
    public string? DeviceName { get; init; }
    [MaxLength(500)]
    public string? DeviceDescription { get; init; }
    [MaxLength(50)]
    public string? WaitTime { get; init; }
}