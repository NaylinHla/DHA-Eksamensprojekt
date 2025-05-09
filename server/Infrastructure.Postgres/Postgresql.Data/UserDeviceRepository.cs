using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class UserDeviceRepository(MyDbContext ctx) : IUserDeviceRepository
{
    public async Task<UserDevice?> GetUserDeviceByIdAsync(Guid userDeviceId)
    {
        return await ctx.UserDevices.FirstOrDefaultAsync(d => d.DeviceId == userDeviceId);
    }

    public async Task<List<UserDevice>> GetAllUserDevicesAsync(Guid userId)
    {
        return await ctx.UserDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    public async Task<Guid> GetUserDeviceOwnerUserIdAsync(Guid deviceId)
    {
        var userId = await ctx.UserDevices
            .Where(d => d.DeviceId == deviceId)
            .Select(d => d.UserId)
            .FirstOrDefaultAsync();

        if (userId == Guid.Empty)
            throw new NotFoundException("Device not found");

        return userId;
    }

    public async Task<UserDevice> CreateUserDeviceAsync(Guid deviceId, UserDevice userDevice)
    {
        userDevice.DeviceId = deviceId;
        ctx.UserDevices.Add(userDevice);
        await ctx.SaveChangesAsync();
        return userDevice;
    }

    public async Task DeleteUserDeviceAsync(Guid deviceId)
    {
        var device = await ctx.UserDevices.FindAsync(deviceId)
                     ?? throw new NotFoundException("Device not found");

        ctx.UserDevices.Remove(device);
        await ctx.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => ctx.SaveChangesAsync();

}