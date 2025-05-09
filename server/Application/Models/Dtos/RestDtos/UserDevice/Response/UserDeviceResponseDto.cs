namespace Application.Models.Dtos.RestDtos.UserDevice.Response;

public class UserDeviceResponseDto
{
    public required Guid DeviceId { get; set; }
    public required Guid UserId { get; set; }
    public required string DeviceName { get; set; }
    public string DeviceDescription { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public required string WaitTime { get; set; }
}