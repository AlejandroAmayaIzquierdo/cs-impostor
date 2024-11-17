


using Fleck;
using WS.Models;
using WS.Services;

namespace WS.Events;

public class GameStateEvent : BaseEventHandler<PlayerUpdatePacket>
{
    public override byte EventType => 0x01;

    public override Task Handle(PlayerUpdatePacket dto, IWebSocketConnection socket)
    {
        StateService.BroadCastClients($"Player {dto.PlayerID} on x: {dto.PositionX}, y: {dto.PositionY}, z: {dto.PositionZ}", socket);
        // Console.WriteLine($"Player {dto.PlayerID} on x: {dto.PositionX}, y: {dto.PositionY}, z: {dto.PositionZ}");
        return Task.CompletedTask;
    }
}