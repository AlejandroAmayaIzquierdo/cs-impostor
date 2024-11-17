


using Fleck;
using WS.Models;
using WS.Services;

namespace WS.Events;

public class GameStateEvent : BaseEventHandler<PlayerUpdatePacket>
{
    public override byte EventType => 0x01;

    public override Task Handle(PlayerUpdatePacket dto, IWebSocketConnection socket)
    {
        using var memoryStream = new MemoryStream();
        using BinaryWriter writer = new(memoryStream);

        byte header = EventType;
        int playerId = dto.PlayerID; // Player ID
        float posX = dto.PositionX, posY = dto.PositionY, posZ = dto.PositionZ; // Position

        byte[] packet;

        writer.Write(header);
        writer.Write(playerId);
        writer.Write(posX); writer.Write(posY); writer.Write(posZ);
        packet = memoryStream.ToArray();


        StateService.BroadCastClients(packet, socket);
        return Task.CompletedTask;
    }
}