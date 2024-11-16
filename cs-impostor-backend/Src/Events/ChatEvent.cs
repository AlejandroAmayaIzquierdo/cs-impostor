
using Fleck;
using WS.Models;

namespace WS.Events;

public class ChatEvent : BaseEventHandler<PlayerMessagePacket>
{
    public override byte EventType => 0x02;

    public override Task Handle(PlayerMessagePacket dto, IWebSocketConnection socket)
    {
        Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}