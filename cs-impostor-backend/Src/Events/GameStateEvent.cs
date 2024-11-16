


using Fleck;
using WS.Models;

namespace WS.Events;

public class GameStateEvent : BaseEventHandler<PlayerUpdatePacket>
{
    public override byte EventType => 0x01;

    public override Task Handle(PlayerUpdatePacket dto, IWebSocketConnection socket)
    {
        Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}