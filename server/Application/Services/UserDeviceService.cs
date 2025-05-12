using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Logging;

namespace Application.Services;

public class UserDeviceService(IUserDeviceRepository userDeviceRepo, IMqttPublisher mqttPublisher) : IUserDeviceService
{
    private const string DeviceNotFound = "Device not found.";
    private const string UnauthorizedDeviceAccess = "You do not own this device.";
    private const string DeviceIdRequired = "DeviceId is required.";

    public async Task<UserDevice?> GetUserDeviceAsync(Guid deviceId, JwtClaims claims)
    {
        MonitorService.Log.Debug($"Entering GetUserDeviceAsync for DeviceId: {deviceId}");

        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new NotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug($"Unauthorized access attempt for DeviceId: {deviceId} by UserId: {claims.Id}");
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        MonitorService.Log.Debug($"Fetched UserDevice for DeviceId: {deviceId} successfully.");
        return device;
    }

    public async Task<List<UserDevice>> GetAllUserDeviceAsync(JwtClaims claims)
    {
        MonitorService.Log.Debug($"Fetching all devices for UserId: {claims.Id}");

        var devices = await userDeviceRepo.GetAllUserDevicesAsync(Guid.Parse(claims.Id));
        MonitorService.Log.Debug($"Fetched {devices.Count} devices for UserId: {claims.Id}");
        return devices;
    }

    public async Task<UserDevice> CreateUserDeviceAsync(UserDeviceCreateDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug("Creating new user device");

        var userDevice = new UserDevice
        {
            DeviceId = Guid.NewGuid(),
            UserId = Guid.Parse(claims.Id),
            DeviceName = dto.DeviceName,
            DeviceDescription = dto.DeviceDescription ?? string.Empty,
            CreatedAt = dto.Created ?? DateTime.UtcNow,
            WaitTime = dto.WaitTime ?? "60" //Default wait time
        };

        var createdDevice = await userDeviceRepo.CreateUserDeviceAsync(userDevice.DeviceId, userDevice);
        MonitorService.Log.Debug($"Created new UserDevice with DeviceId: {createdDevice.DeviceId}");
        return createdDevice;
    }

    public async Task<UserDevice> UpdateUserDeviceAsync(Guid deviceId, UserDeviceEditDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug($"Updating UserDevice with DeviceId: {deviceId}");

        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new NotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug($"Unauthorized access attempt for DeviceId: {deviceId} by UserId: {claims.Id}");
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        if (dto.DeviceName is not null) device.DeviceName = dto.DeviceName;
        if (dto.DeviceDescription is not null) device.DeviceDescription = dto.DeviceDescription;
        if (dto.WaitTime is not null) device.WaitTime = dto.WaitTime;

        await userDeviceRepo.SaveChangesAsync();
        MonitorService.Log.Debug($"Updated UserDevice with DeviceId: {deviceId}");
        return device;
    }

    public async Task DeleteUserDeviceAsync(Guid deviceId, JwtClaims claims)
    {
        MonitorService.Log.Debug($"Deleting UserDevice with DeviceId: {deviceId}");

        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new KeyNotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug($"Unauthorized delete attempt for DeviceId: {deviceId} by UserId: {claims.Id}");
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        await userDeviceRepo.DeleteUserDeviceAsync(deviceId);
        MonitorService.Log.Debug($"Deleted UserDevice with DeviceId: {deviceId}");
    }

    public async Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug($"Entering UpdateDeviceFeed for DeviceId: {dto.DeviceId}");

        if (string.IsNullOrEmpty(dto.DeviceId))
            throw new ArgumentException(DeviceIdRequired);

        var device = await userDeviceRepo.GetUserDeviceByIdAsync(Guid.Parse(dto.DeviceId))
                     ?? throw new NotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug($"Unauthorized access attempt for DeviceId: {dto.DeviceId} by UserId: {claims.Id}");
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        await mqttPublisher.Publish(dto,
            $"{StringConstants.Device}/{dto.DeviceId}/{StringConstants.ChangePreferences}");

        // Update fields if they are not null
        if (dto.Interval is not null)
            device.WaitTime = dto.Interval;

        await userDeviceRepo.SaveChangesAsync();
        MonitorService.Log.Debug($"Device preferences updated for DeviceId: {dto.DeviceId}");
    }
}