

using System.Reflection;
using System.Text;
using Fleck;
using WS.Models;
using WS.Util;

namespace WS.Events;

public abstract class BaseEventHandler<T> where T : BaseEvent
{
    public abstract byte EventType { get; }
    public async Task InvokeHandle(BinaryReader reader, IWebSocketConnection socket)
    {
        // Ensure `T` has a parameterless constructor
        if (Activator.CreateInstance(typeof(T)) is not T dto)
            throw new Exception("Unable to create instance of type T");


        var properties = typeof(T).GetProperties().Where(p => p.Name != "Header");
        var propertiesInfo = new List<(PropertyInfo Property, BinaryData Attribute)>();


        // Collect properties with BinaryData attribute
        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<BinaryData>();
            if (attr != null)
            {
                if (propertiesInfo.Any(e => e.Attribute.Index == attr.Index))
                    throw new Exception("Cannot have duplicate indexes.");

                propertiesInfo.Add((prop, attr));
            }
        }

        // Sort properties by BinaryData index
        propertiesInfo = [.. propertiesInfo.OrderBy(p => p.Attribute.Index)];

        // Populate the dto object
        foreach (var (property, attr) in propertiesInfo)
        {
            object? value = attr.Size switch
            {
                8 => reader.ReadByte(),
                16 => reader.ReadUInt16(),
                32 => reader.ReadUInt32(),
                -1 => ReadStringToEnd(reader),
                _ => null
            };

            if (value != null)
                property.SetValue(dto, Convert.ChangeType(value, property.PropertyType));
            Console.WriteLine(value);
        }
        dto.Header = EventType;
        await Handle(dto, socket);
    }

    private string ReadStringToEnd(BinaryReader reader)
    {
        // Calculate how many bytes are left in the stream
        long remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;

        if (remainingBytes <= 0)
            throw new Exception("No data remaining to read for string.");

        // Read the remaining bytes as a string
        byte[] byteArray = reader.ReadBytes((int)remainingBytes);

        // Convert to a string using UTF-8 encoding
        string result = Encoding.UTF8.GetString(byteArray).Trim();

        // Remove any control characters from the start of the string
        result = new string(result.Where(c => !char.IsControl(c)).ToArray());

        return result;
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);

}