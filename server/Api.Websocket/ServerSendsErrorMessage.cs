using WebSocketBoilerplate;

namespace Api.Websocket;

public class ServerSendsErrorMessage : BaseDto
{
    public required string Message { get; set; }
}