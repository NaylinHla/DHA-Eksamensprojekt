using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Logging;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class UserDeviceRepository(MyDbContext ctx) : IUserDeviceRepository
{
    public async Task<UserDevice?> GetUserDeviceByIdAsync(Guid userDeviceId)
    {
        MonitorService.Log.Debug("Entering GetUserDeviceByIdAsync with DeviceId: {DeviceId}", userDeviceId);

        var device = await ctx.UserDevices.FirstOrDefaultAsync(d => d.DeviceId == userDeviceId);

        MonitorService.Log.Debug(
            device == null
                ? "Device not found with DeviceId: {DeviceId}"
                : "Fetched UserDevice with DeviceId: {DeviceId}", userDeviceId);

        return device;
    }

    public async Task<List<UserDevice>> GetAllUserDevicesAsync(Guid userId)
    {
        MonitorService.Log.Debug("Fetching all devices for UserId: {UserId}", userId);

        var devices = await ctx.UserDevices
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .ToListAsync();

        MonitorService.Log.Debug("Fetched {Count} devices for UserId: {UserId}", devices.Count, userId);

        return devices;
    }

    public async Task<Guid> GetUserDeviceOwnerUserIdAsync(Guid deviceId)
    {
        MonitorService.Log.Debug("Fetching owner for DeviceId: {DeviceId}", deviceId);

        var userId = await ctx.UserDevices
            .Where(d => d.DeviceId == deviceId)
            .Select(d => d.UserId)
            .FirstOrDefaultAsync();

        if (userId == Guid.Empty)
        {
            MonitorService.Log.Debug("Device not found for DeviceId: {DeviceId}", deviceId);
            throw new NotFoundException("Device not found");
        }

        MonitorService.Log.Debug("Fetched UserId: {UserId} for DeviceId: {DeviceId}", userId, deviceId);

        return userId;
    }

    public async Task<UserDevice> CreateUserDeviceAsync(Guid deviceId, UserDevice userDevice)
    {
        MonitorService.Log.Debug("Creating UserDevice with DeviceId: {DeviceId}", deviceId);

        userDevice.DeviceId = deviceId;
        ctx.UserDevices.Add(userDevice);
        await ctx.SaveChangesAsync();

        MonitorService.Log.Debug("Created UserDevice with DeviceId: {DeviceId}", deviceId);

        return userDevice;
    }

    public async Task DeleteUserDeviceAsync(Guid deviceId)
    {
        MonitorService.Log.Debug("Deleting UserDevice with DeviceId: {DeviceId}", deviceId);

        var device = await ctx.UserDevices.FindAsync(deviceId)
                     ?? throw new NotFoundException("Device not found");

        ctx.UserDevices.Remove(device);
        await ctx.SaveChangesAsync();

        MonitorService.Log.Debug("Deleted UserDevice with DeviceId: {DeviceId}", deviceId);
    }

    public Task SaveChangesAsync()
    {
        MonitorService.Log.Debug("Saving changes to the database");
        return ctx.SaveChangesAsync();
    }
}