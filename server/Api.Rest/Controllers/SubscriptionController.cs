using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class SubscriptionController(
    ISecurityService securityService,
    IConnectionManager connectionManager,
    IWebsocketSubscriptionService websocketSubscriptionService) : ControllerBase
{
    public const string SubscriptionRoute = nameof(Subscribe);

    public const string UnsubscribeRoute = nameof(Unsubscribe);

    public const string ExampleBroadcastRoute = nameof(ExampleBroadcast);

    [HttpPost]
    [Route(SubscriptionRoute)]
    public async Task<ActionResult> Subscribe([FromHeader] string authorization, [FromBody] ChangeSubscriptionDto dto)
    {
        await websocketSubscriptionService.SubscribeToTopic(dto.ClientId, dto.TopicIds);
        return Ok();
    }

    [HttpPost]
    [Route(UnsubscribeRoute)]
    public async Task<ActionResult> Unsubscribe([FromHeader] string authorization, [FromBody] ChangeSubscriptionDto dto)
    {
        await websocketSubscriptionService.UnsubscribeFromTopic(dto.ClientId, dto.TopicIds);
        return Ok();
    }

    [HttpPost]
    [Route(ExampleBroadcastRoute)]
    public async Task<ActionResult> ExampleBroadcast([FromBody] ExampleBroadcastDto dto)
    {
        await connectionManager.BroadcastToTopic("ExampleTopic", dto);
        return Ok();
    }
}

public class ExampleBroadcastDto
{
    public string eventType { get; set; } = nameof(ExampleBroadcastDto);
    public required string Message { get; set; }
}