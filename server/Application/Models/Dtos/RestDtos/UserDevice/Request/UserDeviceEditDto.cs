using System.ComponentModel.DataAnnotations;

namespace Application.Models.Dtos.RestDtos.UserDevice.Request;

public class UserDeviceEditDto
{
    [MaxLength(50)] [MinLength(2)] public string? DeviceName { get; init; }

    [MaxLength(500)] public string? DeviceDescription { get; init; }

    [Range(10, int.MaxValue)] public string? WaitTime { get; init; }
}