using System.Net;
using MQTTnet;
using MQTTnet.Server;
using Microsoft.Extensions.Configuration;
using MQTTnet.Protocol;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var brokerConfig = configuration.GetSection("Broker");
string host = brokerConfig["Host"] ?? "0.0.0.0";
int port = int.TryParse(brokerConfig["Port"], out var p) ? p : 1883;
bool useTls = bool.TryParse(brokerConfig["Tls"], out var t) && t;
bool allowAnonymous = !bool.TryParse(brokerConfig["AllowAnonymous"], out var aa) || aa;
string? username = brokerConfig["Username"];
string? password = brokerConfig["Password"];
int maxPendingMessages = int.TryParse(brokerConfig["MaxPendingMessages"], out var mpm) ? mpm : 1000;

var mqttFactory = new MqttFactory();
var mqttServerOptionsBuilder = new MqttServerOptionsBuilder()
    .WithDefaultEndpoint()
    .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(host))
    .WithDefaultEndpointPort(port)
    .WithMaxPendingMessagesPerClient(maxPendingMessages);

if (useTls)
{
    mqttServerOptionsBuilder = mqttServerOptionsBuilder.WithEncryptedEndpoint();
}

var options = mqttServerOptionsBuilder.Build();
var mqttServer = mqttFactory.CreateMqttServer(options);

if (!allowAnonymous)
{
    mqttServer.ValidatingConnectionAsync += async context =>
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            context.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return;
        }

        if (context.UserName != username || context.Password != password)
        {
            context.ReasonCode = MqttConnectReasonCode.NotAuthorized;
            return;
        }

        context.ReasonCode = MqttConnectReasonCode.Success;
    };
}

mqttServer.ClientConnectedAsync += async e =>
{
    Console.WriteLine($"Client connected: {e.ClientId}");
};

mqttServer.ClientDisconnectedAsync += async e =>
{
    Console.WriteLine($"Client disconnected: {e.ClientId}");
};

mqttServer.InterceptingPublishAsync += async e =>
{
    Console.WriteLine($"[{DateTimeOffset.Now}] {e.ClientId} -> {e.ApplicationMessage.Topic}");
};

await mqttServer.StartAsync();
Console.WriteLine($"MQTT Broker started on {host}:{port} (TLS={(useTls ? "on" : "off")})");

Console.CancelKeyPress += async (_, ea) =>
{
    ea.Cancel = true;
    Console.WriteLine("Stopping MQTT Broker...");
    await mqttServer.StopAsync();
};

await Task.Delay(Timeout.Infinite);
