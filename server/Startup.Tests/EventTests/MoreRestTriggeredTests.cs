/*
using System.Text.Json;
using Api.Rest.Controllers;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Services;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup;
using Startup.Tests;
using Startup.Tests.TestUtils;
using WebSocketBoilerplate;

[TestFixture]
public class MoreRestTriggeredTests
{
    [SetUp]
    public void Setup()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig();
                    
                });
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
    public async Task WhenAdminDeletesData_AllWsClientsAreNotified_AndDataIsGoneInDatabase()
    {
        //Arrange
        var wsClient1 = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        var wsClient2 = new TestWsClient();

        var connManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        await connManager.AddToTopic(StringConstants.Dashboard, wsClient1.WsClientId);
        await connManager.AddToTopic(StringConstants.Dashboard, wsClient2.WsClientId);

        var dbCtx = _scopedServiceProvider.GetRequiredService<MyDbContext>();
        dbCtx.Devicelogs.Add(new Devicelog()
        {
            Timestamp = DateTime.UtcNow,
            Deviceid = Guid.NewGuid().ToString(),
            Unit = "Celcius",
            Value = 100,
            Id = Guid.NewGuid().ToString()
        });
        dbCtx.SaveChanges();

        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        
        //Act
        _ = await _httpClient.DeleteAsync(WeatherStationController.DeleteDataRoute);

        await Task.Delay(1000);
        
        //Assertion
        var numberOfRows = dbCtx.Devicelogs.Count();
        if (numberOfRows != 0)
            throw new Exception("There should be no device logs after deletion!!");
       
        var receivedDtos = wsClient1.ReceivedMessages
            .Select(str => JsonSerializer.Deserialize<BaseDto>(str));

        var filtered = receivedDtos.Where(dto => dto.eventType == nameof(AdminHasDeletedData));
        if (filtered.Count() != 1)
            throw new Exception("There should be exactly one broadcast of the type " + nameof(AdminHasDeletedData)+ " but we received all of this data in the array: "+JsonSerializer.Serialize(filtered));
        
        var receivedDtosForClient2 = wsClient2.ReceivedMessages
            .Select(str => JsonSerializer.Deserialize<BaseDto>(str));

        var filtered2 = receivedDtosForClient2.Where(dto => dto.eventType == nameof(AdminHasDeletedData));
        if (filtered2.Count() != 1)
            throw new Exception("There should be exactly one broadcast of the type " + nameof(AdminHasDeletedData));
    }
}*/