using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Logging;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class GreenhouseDeviceRepository(MyDbContext ctx) : IGreenhouseDeviceRepository
{
    // Fetch the recent sensor history logs
    public List<SensorHistory> GetRecentSensorHistory()
    {
        MonitorService.Log.Debug("Fetching recent sensor history from the database");

        // Assuming we want the most recent history based on the timestamp
        var sensorHistory = ctx.SensorHistories.OrderByDescending(sh => sh.Time).ToList();

        MonitorService.Log.Debug("Fetched {Count} recent sensor history records", sensorHistory.Count);
        return sensorHistory;
    }

    public async Task<Guid> GetDeviceOwnerUserId(Guid deviceId)
    {
        MonitorService.Log.Debug("Fetching device owner for DeviceId: {DeviceId}", deviceId);

        var device = await ctx.UserDevices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        if (device == null)
        {
            MonitorService.Log.Warning("Device with DeviceId: {DeviceId} not found", deviceId);
            throw new NotFoundException("Device not found");
        }

        MonitorService.Log.Debug("Found device with DeviceId: {DeviceId}, OwnerId: {UserId}", deviceId, device.UserId);
        return device.UserId;
    }

    public async Task<List<SensorHistoryWithDeviceDto>> GetLatestSensorDataForUserDevicesAsync(Guid userId)
    {
        MonitorService.Log.Debug("Fetching latest sensor data for UserId: {UserId}", userId);

        var records = await ctx.UserDevices
            .Where(device => device.UserId == userId)
            .Select(device => new SensorHistoryWithDeviceDto
            {
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                DeviceCreateDateTime = device.CreatedAt,
                DeviceDesc = device.DeviceDescription,
                DeviceWaitTime = device.WaitTime,
                Temperature = device.SensorHistories.OrderByDescending(s => s.Time).Select(s => s.Temperature).FirstOrDefault(),
                Humidity = device.SensorHistories.OrderByDescending(s => s.Time).Select(s => s.Humidity).FirstOrDefault(),
                AirPressure = device.SensorHistories.OrderByDescending(s => s.Time).Select(s => s.AirPressure).FirstOrDefault(),
                AirQuality = device.SensorHistories.OrderByDescending(s => s.Time).Select(s => s.AirQuality).FirstOrDefault(),
                Time = device.SensorHistories.OrderByDescending(s => s.Time).Select(s => s.Time).FirstOrDefault()
            })
            .ToListAsync();

        MonitorService.Log.Debug("Fetched {Count} sensor data records for UserId: {UserId}", records.Count, userId);
        return records;
    }

    public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceIdAsync(
        Guid deviceId, DateTime? from = null, DateTime? to = null)
    {
        MonitorService.Log.Debug("Fetching sensor history for DeviceId: {DeviceId}", deviceId);

        // Fetch device information
        var device = await ctx.UserDevices.FirstOrDefaultAsync(ud => ud.DeviceId == deviceId);
        if (device == null)
        {
            MonitorService.Log.Warning("Device with DeviceId: {DeviceId} not found", deviceId);
            throw new NotFoundException("Device not found");
        }

        var query = ctx.SensorHistories.Where(sh => sh.DeviceId == deviceId);
        if (from.HasValue)
            query = query.Where(sh => sh.Time >= from.Value);

        if (to.HasValue)
            query = query.Where(sh => sh.Time <= to.Value);

        var sensorHistoryRecords = await query
            .OrderBy(sh => sh.Time)
            .Select(sh => new SensorHistoryDto
            {
                Temperature = sh.Temperature,
                Humidity = sh.Humidity,
                AirPressure = sh.AirPressure,
                AirQuality = sh.AirQuality,
                Time = sh.Time
            })
            .ToListAsync();

        var responseDto = new GetAllSensorHistoryByDeviceIdDto
        {
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            SensorHistoryRecords = sensorHistoryRecords
        };

        MonitorService.Log.Debug("Fetched {Count} records for DeviceId: {DeviceId}", sensorHistoryRecords.Count, deviceId);
        return new List<GetAllSensorHistoryByDeviceIdDto> { responseDto };
    }

    // Add a new sensor history log
    public async Task<SensorHistory> AddSensorHistory(SensorHistory sensorHistory)
    {
        MonitorService.Log.Debug("Adding new sensor history to the database");

        ctx.SensorHistories.Add(sensorHistory);
        await ctx.SaveChangesAsync();

        MonitorService.Log.Debug("Added new sensor history record for DeviceId: {DeviceId}", sensorHistory.DeviceId);
        return sensorHistory;
    }

    // Fetch user by device ID
    public async Task<User> GetUserByDeviceId(Guid deviceId)
    {
        MonitorService.Log.Debug("Fetching user for DeviceId: {DeviceId}", deviceId);

        var device = await ctx.UserDevices
            .Include(userDevice => userDevice.User)
            .FirstOrDefaultAsync(ud => ud.DeviceId == deviceId);

        if (device == null)
        {
            MonitorService.Log.Warning("Device with DeviceId: {DeviceId} not found", deviceId);
            throw new NotFoundException("Device not found");
        }

        if (device.User == null)
        {
            MonitorService.Log.Warning("User for DeviceId: {DeviceId} not found", deviceId);
            throw new NotFoundException("Device User not found");
        }

        MonitorService.Log.Debug("Fetched user for DeviceId: {DeviceId}, UserId: {UserId}", deviceId, device.User.UserId);
        return device.User;
    }

    // Delete all sensor history data from a specific device
    public async Task DeleteDataFromSpecificDevice(Guid deviceId)
    {
        MonitorService.Log.Debug("Deleting sensor data for DeviceId: {DeviceId}", deviceId);

        var deviceSensorHistory = await ctx.SensorHistories
            .Where(sh => sh.DeviceId == deviceId)
            .ToListAsync();

        ctx.SensorHistories.RemoveRange(deviceSensorHistory);
        await ctx.SaveChangesAsync();

        MonitorService.Log.Debug("Deleted {Count} sensor history records for DeviceId: {DeviceId}", deviceSensorHistory.Count, deviceId);
    }
}