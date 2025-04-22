using Application.Models.Enums;
using Core.Domain.Entities;

namespace Startup.Tests.TestUtils;

public static class MockObjects
{
    public static User GetUser(string? role = null)
    {
        return new User
        {
            Role = role ?? Constants.UserRole,
            Email = "test" + Guid.NewGuid() + "@test.com",
            Id = Guid.NewGuid().ToString(),
            Salt = "word", //password is "pass" and the hash is the combined pass + word hashed together 
            Hash =
                "b109f3bbbc244eb82441917ed06d618b9008dd09b3befd1b5e07394c706a8bb980b1d7785e5976ec049b46df5f1326af5a2ea6d103fd07c95385ffab0cacbc86"
        };
    }
}