namespace Application.Models.Dtos.RestDtos.UserDevice
{
    public class GetAllUserDeviceDto
    {
        public List<UserDevice> AllUserDevice { get; set; } = new();
    }
    
    public class UserDevice
    {
        public Guid DeviceId { get; set; }
        public Guid UserId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceDescription { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}