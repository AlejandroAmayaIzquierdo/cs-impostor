using System.Reflection;
using System.Text.Json;
using Fleck;
using lib;
using WS.Events;
using WS.Models;
using WS.Services;

namespace WS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var allowedHosts =
            builder
                .Configuration.GetValue<string>("AllowedHosts")
                ?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (allowedHosts.Contains("*"))
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                else
                    policy.WithOrigins(allowedHosts).AllowAnyMethod().AllowAnyHeader();
            });
        });

        var services = builder.FindAndInjectClientBinaryEventHandlers(Assembly.GetExecutingAssembly());

        var app = builder.Build();

        string? webSocketConnection =
            builder.Configuration.GetConnectionString("WebSocket")
            ?? throw new Exception("The connection string of WebSocket should be stablish");

        var server = new WebSocketServer(webSocketConnection);

        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                if (socket.ConnectionInfo.Headers.TryGetValue("Origin", out string? value))
                {
                    var (isJoined, outMsg) = StateService.Game.TryJoin(socket);

                    if (!isJoined)
                    {
                        // FIXME send error code or something that is not string
                        socket.Send("Error");
                        return;
                    }
                    using var memoryStream = new MemoryStream();
                    using BinaryWriter writer = new(memoryStream);

                    bool isParse = int.TryParse(outMsg, out int playerID);

                    if (!isParse)
                    {
                        socket.Send("Error");
                        return;
                    }

                    writer.Write(playerID);
                    byte[] pack = memoryStream.ToArray();
                    socket.Send(pack);

                }
            };
            socket.OnBinary = (data) =>
            {
                // FIXME Global error handling
                app.InvokeEventHandlerBinaryData(services, socket, data);

            };
            socket.OnMessage = async (message) =>
            {
                try
                {
                    await app.InvokeClientEventHandler(services, socket, message);
                }
                catch
                {
                    StateService.Game.Leave(socket.ConnectionInfo.Id);
                    await socket.Send("Error");
                }
            };
            socket.OnClose = () =>
            {
                StateService.Game.Leave(socket.ConnectionInfo.Id);

            };
        });

        Console.ReadLine();

    }
}
