

namespace WS.Util;

[AttributeUsage(AttributeTargets.All)]
public class BinaryData(int index, int size) : Attribute
{
    public int Index { get; set; } = index;
    public int Size { get; set; } = size;
}
