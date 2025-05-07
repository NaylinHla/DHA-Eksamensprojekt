using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.RestDtos.SensorHistory;
using Application.Models.Dtos.RestDtos.UserDevice.Response;
using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using UserDevice = Application.Models.Dtos.RestDtos.UserDevice.Response.UserDevice;

namespace Infrastructure.Postgres.Postgresql.Data
{
    public class GreenhouseDeviceRepository(MyDbContext ctx) : IGreenhouseDeviceRepository
    {
        // Fetch the recent sensor history logs
        public List<SensorHistory> GetRecentSensorHistory()
        {
            // Assuming we want the most recent history based on the timestamp
            return ctx.SensorHistories.OrderByDescending(sh => sh.Time).ToList();
        }

        public async Task<Guid> GetDeviceOwnerUserId(Guid deviceId)
        {
            var device = await ctx.UserDevices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);

            if (device == null)
                throw new NotFoundException("Device not found");
            
            return device.UserId;
        }

        public async Task<List<SensorHistoryWithDeviceDto>> GetLatestSensorDataForUserDevicesAsync(Guid userId)
        {
            return await ctx.UserDevices
                .Where(device => device.UserId == userId)
                .Select(device => new SensorHistoryWithDeviceDto
                {
                    DeviceId = device.DeviceId,
                    DeviceName = device.DeviceName,
                    DeviceCreateDateTime = device.CreatedAt,
                    DeviceDesc = device.DeviceDescription,
                    Temperature = device.SensorHistories
                        .OrderByDescending(s => s.Time)
                        .Select(s => s.Temperature)
                        .FirstOrDefault(),
                    Humidity = device.SensorHistories
                        .OrderByDescending(s => s.Time)
                        .Select(s => s.Humidity)
                        .FirstOrDefault(),
                    AirPressure = device.SensorHistories
                        .OrderByDescending(s => s.Time)
                        .Select(s => s.AirPressure)
                        .FirstOrDefault(),
                    AirQuality = device.SensorHistories
                        .OrderByDescending(s => s.Time)
                        .Select(s => s.AirQuality)
                        .FirstOrDefault(),
                    Time = device.SensorHistories
                        .OrderByDescending(s => s.Time)
                        .Select(s => s.Time)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }
        
        public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceIdAsync(Guid deviceId, DateTime? from = null, DateTime? to = null)
        {
            // Fetch device information
            var device = await ctx.UserDevices
                .FirstOrDefaultAsync(ud => ud.DeviceId == deviceId);

            if (device == null)
                throw new NotFoundException("Device not found");

            var query = ctx.SensorHistories
                .Where(sh => sh.DeviceId == deviceId);

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

            // Map into your response DTO
            var responseDto = new GetAllSensorHistoryByDeviceIdDto
            {
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                SensorHistoryRecords = sensorHistoryRecords
            };

            return new List<GetAllSensorHistoryByDeviceIdDto> { responseDto };
        }


        // Add a new sensor history log
        public async Task<SensorHistory> AddSensorHistory(SensorHistory sensorHistory)
        {
            ctx.SensorHistories.Add(sensorHistory);
            await ctx.SaveChangesAsync();
            return sensorHistory;
        }

        // Fetch user by device ID (You can use this if you need to fetch user details)
        public async Task<User> GetUserByDeviceId(Guid deviceId)
        {
            // Assuming each device has a unique device ID, and a user is associated with it
            var device = await ctx.UserDevices.Include(userDevice => userDevice.User)
                .FirstOrDefaultAsync(ud => ud.DeviceId == deviceId);
            
            if (device == null)
                throw new NotFoundException("Device not found");
            if (device.User == null)
            {
                throw new NotFoundException("Device User not found"); 
            }
            return device.User;
        }
        
        // Delete all sensor history data from a specific device
        public async Task DeleteDataFromSpecificDevice(Guid deviceId)
        {
            var deviceSensorHistory = await ctx.SensorHistories
                .Where(sh => sh.DeviceId == deviceId)
                .ToListAsync();

            ctx.SensorHistories.RemoveRange(deviceSensorHistory);
            await ctx.SaveChangesAsync();
        }
    }
}