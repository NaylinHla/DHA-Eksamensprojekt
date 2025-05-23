using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using FluentValidation;
using Infrastructure.Logging;

namespace Application.Services;

public class UserDeviceService(
    IUserDeviceRepository userDeviceRepo, 
    IMqttPublisher mqttPublisher,
    IValidator<UserDeviceCreateDto> userDeviceCreateValidator,
    IValidator<UserDeviceEditDto> userDeviceEditValidator,
    IValidator<AdminChangesPreferencesDto> adminChangesPreferencesValidator) : IUserDeviceService
{
    private const string DeviceNotFound = "Device not found.";
    private const string UnauthorizedDeviceAccess = "You do not own this device.";

    public async Task<UserDevice?> GetUserDeviceAsync(Guid deviceId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entering GetUserDeviceAsync for DeviceId: {DeviceId}", deviceId);

        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new NotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug("Unauthorized access attempt for DeviceId: {DeviceId} by UserId: {UserId}", deviceId, claims.Id);
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        MonitorService.Log.Debug("Fetched UserDevice for DeviceId: {DeviceId} successfully", deviceId);
        return device;
    }

    public async Task<List<UserDevice>> GetAllUserDeviceAsync(JwtClaims claims)
    {
        MonitorService.Log.Debug("Fetching all devices for UserId: {UserId}", claims.Id);

        var devices = await userDeviceRepo.GetAllUserDevicesAsync(Guid.Parse(claims.Id));
        MonitorService.Log.Debug("Fetched {Count} devices for UserId: {UserId}", devices.Count, claims.Id);
        return devices;
    }

    public async Task<UserDevice> CreateUserDeviceAsync(UserDeviceCreateDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug("Creating new user device");

        await userDeviceCreateValidator.ValidateAndThrowAsync(dto);
        
        var userDevice = new UserDevice
        {
            DeviceId = Guid.NewGuid(),
            UserId = Guid.Parse(claims.Id),
            DeviceName = dto.DeviceName,
            DeviceDescription = dto.DeviceDescription ?? string.Empty,
            CreatedAt = dto.Created ?? DateTime.UtcNow,
            WaitTime = dto.WaitTime ?? "60" // Default wait time
        };

        var createdDevice = await userDeviceRepo.CreateUserDeviceAsync(userDevice.DeviceId, userDevice);
        MonitorService.Log.Debug("Created new UserDevice with DeviceId: {DeviceId}", createdDevice.DeviceId);
        return createdDevice;
    }

    public async Task<UserDevice> UpdateUserDeviceAsync(Guid deviceId, UserDeviceEditDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug("Updating UserDevice with DeviceId: {DeviceId}", deviceId);

        await userDeviceEditValidator.ValidateAndThrowAsync(dto);
        
        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new NotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug("Unauthorized access attempt for DeviceId: {DeviceId} by UserId: {UserId}", deviceId, claims.Id);
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        if (dto.DeviceName is not null) device.DeviceName = dto.DeviceName;
        if (dto.DeviceDescription is not null) device.DeviceDescription = dto.DeviceDescription;
        if (dto.WaitTime is not null) device.WaitTime = dto.WaitTime;

        await userDeviceRepo.SaveChangesAsync();
        MonitorService.Log.Debug("Updated UserDevice with DeviceId: {DeviceId}", deviceId);
        return device;
    }

    public async Task DeleteUserDeviceAsync(Guid deviceId, JwtClaims claims)
    {
        MonitorService.Log.Debug("Deleting UserDevice with DeviceId: {DeviceId}", deviceId);

        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new KeyNotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug("Unauthorized delete attempt for DeviceId: {DeviceId} by UserId: {UserId}", deviceId, claims.Id);
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        await userDeviceRepo.DeleteUserDeviceAsync(deviceId);
        MonitorService.Log.Debug("Deleted UserDevice with DeviceId: {DeviceId}", deviceId);
    }

    public async Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        MonitorService.Log.Debug("Entering UpdateDeviceFeed for DeviceId: {DeviceId}", dto.DeviceId);

        await adminChangesPreferencesValidator.ValidateAndThrowAsync(dto);


        var device = await userDeviceRepo.GetUserDeviceByIdAsync(Guid.Parse(dto.DeviceId))
                     ?? throw new NotFoundException(DeviceNotFound);

        if (device.UserId != Guid.Parse(claims.Id))
        {
            MonitorService.Log.Debug("Unauthorized access attempt for DeviceId: {DeviceId} by UserId: {UserId}", dto.DeviceId, claims.Id);
            throw new UnauthorizedAccessException(UnauthorizedDeviceAccess);
        }

        await mqttPublisher.Publish(dto, $"{StringConstants.Device}/{dto.DeviceId}/{StringConstants.ChangePreferences}");

        // Update fields if they are not null
        if (dto.Interval is not null)
            device.WaitTime = dto.Interval;

        await userDeviceRepo.SaveChangesAsync();
        MonitorService.Log.Debug("Device preferences updated for DeviceId: {DeviceId}", dto.DeviceId);
    }
}