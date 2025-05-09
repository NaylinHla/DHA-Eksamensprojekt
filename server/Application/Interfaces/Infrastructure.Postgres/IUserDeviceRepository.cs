using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IUserDeviceRepository
{
    Task<UserDevice?> GetUserDeviceByIdAsync(Guid userDeviceId);
    Task<List<UserDevice>> GetAllUserDevicesAsync(Guid userId);
    Task SaveChangesAsync();
    Task<Guid> GetUserDeviceOwnerUserIdAsync(Guid deviceId);
    Task<UserDevice> CreateUserDeviceAsync(Guid deviceId, UserDevice userDevice);
    Task DeleteUserDeviceAsync(Guid deviceId);
}