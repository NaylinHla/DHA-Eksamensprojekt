using Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests;

public class SecurityServiceTests
{
    [SetUp]
    public void Setup()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => { services.DefaultTestConfig(makeMqttClient: false); });
            });

        _httpClient = factory.CreateClient();
        _scopedServiceProvider = factory.Services.CreateScope().ServiceProvider;
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [Test]
    public async Task Hash_Can_Correctly_Hash_A_String()
    {
        var expected =
            "b109f3bbbc244eb82441917ed06d618b9008dd09b3befd1b5e07394c706a8bb980b1d7785e5976ec049b46df5f1326af5a2ea6d103fd07c95385ffab0cacbc86";
        var securityService = _scopedServiceProvider.GetRequiredService<ISecurityService>();
        var hash = securityService.HashPassword("password");
        if (hash != expected)
            throw new Exception("Did not create the expected SHA512 hash. Got: " + hash + " and expected: " +
                                expected);
    }
}