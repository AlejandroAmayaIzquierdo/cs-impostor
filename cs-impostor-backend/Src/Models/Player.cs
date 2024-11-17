using System.Numerics;
using Fleck;

namespace WS.Models;


public class Player
{
    public int PlayerID;
    public Vector3 position;
    public required IWebSocketConnection socket;
}