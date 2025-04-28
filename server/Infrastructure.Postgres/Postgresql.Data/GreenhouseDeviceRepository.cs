using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.MqttDtos.Respone;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

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
                throw new FileNotFoundException("Device not found.");

            return device.UserId;
        }

        public List<GetAllSensorHistoryByDeviceIdDto> GetSensorHistoryByDeviceId(Guid deviceId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<GetAllSensorHistoryByDeviceIdDto>> GetSensorHistoryByDeviceIdAsync(Guid deviceId)
        {
            // Fetch device information
            var device = await ctx.UserDevices
                .FirstOrDefaultAsync(ud => ud.DeviceId == deviceId);

            if (device == null)
                throw new Exception("Device not found");

            // Fetch sensor history records for the device
            var sensorHistoryRecords = await ctx.SensorHistories
                .Where(sh => sh.DeviceId == deviceId)
                .OrderBy(sh => sh.Time) // if you want oldest first
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

            return device.User;
        }

        // Delete all sensor history data
        public async Task DeleteAllSensorHistoryData()
        {
            var allSensorHistory = await ctx.SensorHistories.ToListAsync();
            ctx.RemoveRange(allSensorHistory);
            await ctx.SaveChangesAsync();
        }
    }
}