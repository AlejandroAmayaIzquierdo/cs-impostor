

using System.Reflection;
using System.Runtime.CompilerServices;
using Fleck;
using WS.Events;

namespace WS;

public static class EventRegister
{

    public static HashSet<Type> FindAndInjectClientBinaryEventHandlers(this WebApplicationBuilder builder, Assembly assemblyReference, ServiceLifetime? lifetime = ServiceLifetime.Singleton)
    {
        HashSet<Type> hashSet = [];
        Type[] types = assemblyReference.GetTypes();
        foreach (Type type in types)
        {
            if (type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(BaseEventHandler<>))
            {
                if (lifetime.Equals(ServiceLifetime.Singleton))
                {
                    builder.Services.AddSingleton(type);
                }
                else if (lifetime.Equals(ServiceLifetime.Scoped))
                {
                    builder.Services.AddScoped(type);
                }

                hashSet.Add(type);
            }
        }

        return hashSet;
    }
    public static void InvokeEventHandlerBinaryData(this WebApplication app, HashSet<Type> types, IWebSocketConnection ws, byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var header = reader.ReadByte();

        foreach (var type in types)
        {
            if (type == null)
                continue;

            using IServiceScope scope = app.Services.CreateScope();

            IServiceProvider serviceProvider = scope.ServiceProvider;
            dynamic? clientEventService = serviceProvider.GetService(type)
            ?? throw new InvalidOperationException($"Could not resolve service for header: {header}");

            if (header != clientEventService.EventType)
                continue;
            dynamic awaiter = clientEventService.InvokeHandle(reader, ws).GetAwaiter();

            if (!(bool)awaiter.IsCompleted)
            {
                ICriticalNotifyCompletion? awaiter2 = awaiter as ICriticalNotifyCompletion;
                awaiter2?.OnCompleted(() =>
                {
                    /// XXX Check if need to be done something here
                });


            }

            awaiter.GetResult();
        }

    }

}