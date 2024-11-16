using WS.Util;

namespace WS.Models;

public class BaseEvent
{
    [BinaryData(0, 8)]
    public byte Header { get; set; }
    [BinaryData(1, 32)]
    public int PlayerID { get; set; }

}
public class PlayerMessagePacket : BaseEvent
{
    [BinaryData(2, -1)]
    public string? Message { get; set; }
}

public class PlayerUpdatePacket : BaseEvent
{
    [BinaryData(2, 64)]
    public long Timestamp { get; set; } // Current timestamp in milliseconds (e.g., Unix timestamp)

    [BinaryData(3, 32)]
    public float PositionX { get; set; } // X Coordinate

    [BinaryData(4, 32)]
    public float PositionY { get; set; } // Y Coordinate

    [BinaryData(5, 32)]
    public float PositionZ { get; set; } // Z Coordinate

    [BinaryData(6, 32)]
    public float RotationPitch { get; set; } // Pitch (rotation along X-axis)

    [BinaryData(7, 32)]
    public float RotationYaw { get; set; } // Yaw (rotation along Y-axis)

    [BinaryData(8, 32)]
    public float RotationRoll { get; set; } // Roll (rotation along Z-axis)

    [BinaryData(9, 32)]
    public float VelocityX { get; set; } // Velocity along X-axis

    [BinaryData(10, 32)]
    public float VelocityY { get; set; } // Velocity along Y-axis

    [BinaryData(11, 32)]
    public float VelocityZ { get; set; } // Velocity along Z-axis

    [BinaryData(12, 8)]
    public byte ActionFlags { get; set; } // Action flags (bitfield)
}
