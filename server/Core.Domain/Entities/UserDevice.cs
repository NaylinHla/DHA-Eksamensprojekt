namespace Core.Domain.Entities
{
    public class UserDevice
    {
        public Guid DeviceId { get; set; }
        public Guid UserId { get; set; }
        public required string DeviceName { get; set; }
        public required string DeviceDescription { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public ICollection<SensorHistory> SensorHistories { get; set; } = new List<SensorHistory>();
    }
}