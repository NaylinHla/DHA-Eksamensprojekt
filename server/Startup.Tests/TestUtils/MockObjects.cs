using Application.Models.Enums;
using Core.Domain.Entities;

namespace Startup.Tests.TestUtils;

public static class MockObjects
{
    public static User GetUser(string? role = null)
    {
        var userId = Guid.NewGuid();
        
        return new User
        {
            UserId = userId,
            Role = role ?? Constants.UserRole,
            Email = $"testing{Guid.NewGuid()}@gmail.com",
            Salt = "word", // password is "pass" and the hash is the combined pass + word hashed together
            Hash = "b109f3bbbc244eb82441917ed06d618b9008dd09b3befd1b5e07394c706a8bb980b1d7785e5976ec049b46df5f1326af5a2ea6d103fd07c95385ffab0cacbc86",
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
                WaitTime = "10",
                Celsius = true,
                ConfirmDialog = false,
                DarkTheme = false,
                SecretMode = false
            },
            UserPlants = new List<UserPlant>(), // Empty list
            Alerts = new List<Alert>(), // Empty list
            UserDevices = new List<UserDevice>() // Empty list
        };
        
    }
}