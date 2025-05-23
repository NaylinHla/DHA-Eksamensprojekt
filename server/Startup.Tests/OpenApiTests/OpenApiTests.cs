using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.OpenApiTests;

[TestFixture]
public class OpenApiTests
{
    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;
    
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
        _httpClient.Dispose();
    }

    [Test]
    public async Task CanGetJsonResponseFromOpenApi()
    {
        var response = await _httpClient.GetAsync("/openapi/v1.json");
        var document = await OpenApiDocument.FromJsonAsync(await response.Content.ReadAsStringAsync());
        Assert.That(document.Paths, Is.Not.Empty);
    }
}