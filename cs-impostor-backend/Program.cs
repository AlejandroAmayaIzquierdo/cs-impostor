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
                    var origin = value;

                    // List of allowed origins (e.g., your website domain)
                    var allowedOrigins = allowedHosts;
                    // Check if the Origin is in the allowed origins list
                    if (allowedHosts.Contains("*"))
                    {
                        StateService.AddConnection(socket);
                        return;
                    }
                    else if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
                    {
                        StateService.AddConnection(socket);
                        return;
                    }
                    socket.Send("No allowed host");
                    socket.Close();
                }
            };
            socket.OnBinary = (data) =>
            {
                app.InvokeEventHandlerBinaryData(services, socket, data);
                // await new ChatEvent().InvokeHandle(reader);

            };
            socket.OnMessage = async (message) =>
            {
                try
                {
                    await app.InvokeClientEventHandler(services, socket, message);
                }
                catch (Exception e)
                {
                    StateService.RemoveConnection(socket.ConnectionInfo.Id);

                    var errorObj = new { error = e.Message };
                    var messageError = JsonSerializer.Serialize(errorObj);
                    await socket.Send(messageError);
                }
            };
            socket.OnClose = () =>
            {
                StateService.RemoveConnection(socket.ConnectionInfo.Id);

            };
        });

        Console.ReadLine();

    }
}
