using Application.Models.Enums;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Startup.Tests.TestUtils;

public static class MockObjects
{
    public static User GetUser(string? role = null)
    {
        var userId = Guid.NewGuid();

        // Create a user instance
        var user = new User
        {
            UserId = userId,
            Role = role ?? Constants.UserRole,
            Email = $"testing{Guid.NewGuid()}@gmail.com",
            Salt = "eea62d44-c613-45b0-be6e-4dfa4f2f4973", // password is "Secret25!" and the hash is the combined pass + word hashed together
            Hash =
                "b8986bb85a370d0f4fa81f85354993a9d79f60e557836c3776d41edb345dc97cca359fecb48ddf04db906e535c0158dfbe4f867a997bbdefe9c26c6c28081611",
            FirstName = "Test",
            LastName = "User",
            Birthday = DateTime.UtcNow.AddYears(-30),
            Country = "TestCountry",
            Weather = new Weather
            {
                UserId = userId,
                City = "Copenhagen",
                Country = "Denmark"
            },
            UserSettings = new UserSettings
            {
                UserId = userId,
                Celsius = true,
                ConfirmDialog = false,
                DarkTheme = false,
                SecretMode = false
            },
            UserPlants = new List<UserPlant>(), // Empty list
            Alerts = new List<Alert>(), // Empty list
            UserDevices = new List<UserDevice>() // Empty list
        };

        // Create and add a test device for the user
        var device = new UserDevice
        {
            DeviceId = Guid.NewGuid(),
            UserId = userId,
            DeviceName = "Test Device",
            DeviceDescription = "Device for testing",
            WaitTime = "600",
            CreatedAt = DateTime.UtcNow,
            SensorHistories = new List<SensorHistory>()
        };

        var emaillist = new EmailList
        {
            Email = user.Email,
            Id = 1
        };

        // Add sample sensor data to the device
        device.SensorHistories.Add(new SensorHistory
        {
            SensorHistoryId = Guid.NewGuid(),
            DeviceId = device.DeviceId,
            Temperature = 24.5f,
            Humidity = 60.2f,
            AirPressure = 1010.3f,
            AirQuality = 1,
            Time = DateTime.UtcNow.AddMinutes(-10)
        });

        device.SensorHistories.Add(new SensorHistory
        {
            SensorHistoryId = Guid.NewGuid(),
            DeviceId = device.DeviceId,
            Temperature = 25.1,
            Humidity = 59.8f,
            AirPressure = 1011.2f,
            AirQuality = 1,
            Time = DateTime.UtcNow
        });

        // Add the device to the user's list of devices
        user.UserDevices.Add(device);

        return user;
    }
    
    
    /// <summary>
    /// Seeds a User + Plant + Device + both ConditionAlerts
    /// into the DB and returns the created User.
    /// </summary>
    public static async Task<User> SeedDbAsync(IServiceProvider services, string? role = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        var user = GetUser(role);
        db.Users.Add(user);
        
        // Add a Plant for testing purposes
        var plant = new Plant
        {
            PlantId = Guid.NewGuid(),
            PlantName = "Testomato",
            PlantType = "Flower",
            Planted = DateTime.UtcNow.AddMonths(-2),
            LastWatered = DateTime.UtcNow.AddDays(-3),
            WaterEvery = 7,
            IsDead = false,
            PlantNotes = "Needs indirect sunlight and weekly watering"
        };
        
        db.Plants.Add(plant);
        // Associate condition alerts with the user's plant (for testing purposes)
        user.UserPlants.Add(new UserPlant { UserId = user.UserId, PlantId = plant.PlantId });
        
        var cap = new ConditionAlertPlant {
            ConditionAlertPlantId = Guid.NewGuid(),
            PlantId = user.UserPlants.First().PlantId,
            WaterNotify           = true
        };
        db.ConditionAlertPlant.Add(cap);
        
        var cad = new ConditionAlertUserDevice {
            ConditionAlertUserDeviceId = Guid.NewGuid(),
            UserDeviceId               = user.UserDevices.First().DeviceId,
            SensorType                 = "Temperature",
            Condition                  = "<=50"
        };
        db.ConditionAlertUserDevice.Add(cad);

        await db.SaveChangesAsync();

        return user;
    }
}