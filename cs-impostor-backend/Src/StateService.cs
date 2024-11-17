using Fleck;

namespace WS.Services;
public static class StateService
{
    private static readonly Dictionary<Guid, IWebSocketConnection> _sockets = [];
    private static readonly Dictionary<string, HashSet<Guid>> _rooms = [];

    public static Dictionary<Guid, IWebSocketConnection> Sockets => _sockets;

    public static Dictionary<string, HashSet<Guid>> Rooms => _rooms;

    public static bool AddConnection(IWebSocketConnection webSocket)
    {
        return _sockets.TryAdd(webSocket.ConnectionInfo.Id, webSocket);
    }

    public static bool RemoveConnection(Guid id)
    {
        try
        {
            // Find all rooms containing the user
            foreach (var room in _rooms.Where(e => e.Value.Contains(id)).ToList())
            {
                room.Value.Remove(id); // Remove user from room
                if (room.Value.Count == 0)
                    _rooms.Remove(room.Key); // Remove the room if it's empty
            }
            // roomContainingUser
            return _sockets.Remove(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }

    }
    public static bool AddRoom(IWebSocketConnection socket, string room)
    {
        if (!_rooms.ContainsKey(room))
            _rooms.Add(room, []);
        return _rooms[room].Add(socket.ConnectionInfo.Id);
    }

    public static void BroadcastToRoom(string room, string message)
    {
        bool doesRoomExist = _rooms.TryGetValue(room, out var guids);
        if (!doesRoomExist || guids == null)
            throw new Exception("Room does not exist");

        foreach (var guid in guids)
        {
            if (_sockets.TryGetValue(guid, out var ws))
                ws.Send(message);
        }
    }

    public static void BroadCastClients(string data, IWebSocketConnection? socketOwner = null)
    {
        foreach (var socket in _sockets)
        {
            if (socketOwner != null && socket.Value.ConnectionInfo.Id == socketOwner.ConnectionInfo.Id)
                continue;
            socket.Value.Send(data);

        }
    }
}