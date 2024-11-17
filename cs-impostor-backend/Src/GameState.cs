using System.Numerics;
using Fleck;
using WS.Models;

namespace WS.Services;

public class GameState
{
    public static int MAX_NUM_PLAYER = 100;
    public readonly Dictionary<Guid, Player> players = [];

    private int playerCount = 0;

    public (bool, string) TryJoin(IWebSocketConnection socket)
    {
        playerCount++;
        if (playerCount >= MAX_NUM_PLAYER)
            return (false, "The server if full");

        Player player =
            new()
            {
                socket = socket,
                PlayerID = playerCount,
                position = new Vector3(0, 0, 0)
            };
        var isAdded = players.TryAdd(socket.ConnectionInfo.Id, player);

        if (!isAdded)
            return (false, "Error while tying to add the player to the game");
        return (true, playerCount.ToString());
    }

    public bool Leave(Guid guid)
    {
        return players.Remove(guid);
    }
}
