using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Application.Services;

public class UserDeviceService(IUserDeviceRepository userDeviceRepo, IMqttPublisher mqttPublisher) : IUserDeviceService
{
    public async Task<UserDevice> GetUserDeviceAsync(Guid deviceId, JwtClaims claims)
    {
        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new NotFoundException("Device not found");

        if (device.UserId != Guid.Parse(claims.Id))
            throw new UnauthorizedAccessException("You do not own this device.");

        return device;
    }

    public async Task<List<UserDevice>> GetAllUserDeviceAsync(JwtClaims claims)
    {
        return await userDeviceRepo.GetAllUserDevicesAsync(Guid.Parse(claims.Id));
    }

    public async Task<UserDevice> CreateUserDeviceAsync(UserDeviceCreateDto dto, JwtClaims claims)
    {
        var userDevice = new UserDevice
        {
            DeviceId = Guid.NewGuid(),
            UserId = Guid.Parse(claims.Id),
            DeviceName = dto.DeviceName,
            DeviceDescription = dto.DeviceDescription ?? string.Empty,
            CreatedAt = dto.Created ?? DateTime.UtcNow,
            WaitTime = dto.WaitTime ?? "60" //Default wait time
        };

        return await userDeviceRepo.CreateUserDeviceAsync(userDevice.DeviceId, userDevice);
    }

    public async Task<UserDevice> UpdateUserDeviceAsync(Guid deviceId, UserDeviceEditDto dto, JwtClaims claims)
    {
        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId) ?? throw new NotFoundException("Device not found");

        if (device.UserId != Guid.Parse(claims.Id))
            throw new UnauthorizedAccessException("You do not own this device.");

        // Update fields if they are not null
        if (dto.DeviceName is not null) device.DeviceName = dto.DeviceName;
        if (dto.DeviceDescription is not null) device.DeviceDescription = dto.DeviceDescription;
        if (dto.WaitTime is not null) device.WaitTime = dto.WaitTime;

        await userDeviceRepo.SaveChangesAsync();
        return device;
    }
    
    public async Task DeleteUserDeviceAsync(Guid deviceId, JwtClaims claims)
    {
        var device = await userDeviceRepo.GetUserDeviceByIdAsync(deviceId)
                     ?? throw new KeyNotFoundException("Device not found");

        if (device.UserId != Guid.Parse(claims.Id))
            throw new UnauthorizedAccessException("You do not own this device.");

        await userDeviceRepo.DeleteUserDeviceAsync(deviceId);
    }

    public Task UpdateDeviceFeed(AdminChangesPreferencesDto dto, JwtClaims claims)
    {
        mqttPublisher.Publish(
            dto,
            $"{StringConstants.Device}/{dto.DeviceId}/{StringConstants.ChangePreferences}"
        );
        return Task.CompletedTask;
    }
}