using Microsoft.Extensions.Options;
using MQTTBrokerService.Models;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTTBrokerService.Services;

/// <summary>
/// Interface for managing the MQTT broker.
/// </summary>
public interface IMqttBrokerManager
{
    /// <summary>
    /// Gets the current status of the broker.
    /// </summary>
    BrokerStatus GetStatus();

    /// <summary>
    /// Gets the list of connected clients.
    /// </summary>
    Task<IEnumerable<ConnectedClient>> GetConnectedClientsAsync();

    /// <summary>
    /// Gets the current broker configuration.
    /// </summary>
    BrokerConfiguration GetConfiguration();

    /// <summary>
    /// Updates the broker configuration.
    /// </summary>
    void UpdateConfiguration(BrokerConfiguration configuration);

    /// <summary>
    /// Starts the MQTT broker.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the MQTT broker.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects a specific client.
    /// </summary>
    Task DisconnectClientAsync(string clientId);
}

/// <summary>
/// Manages the MQTT broker lifecycle and operations.
/// </summary>
public class MqttBrokerManager : IMqttBrokerManager
{
    private readonly ILogger<MqttBrokerManager> _logger;
    private MqttServer? _mqttServer;
    private BrokerConfiguration _configuration;
    private DateTime? _startedAt;
    private int _connectedClientCount;
    private readonly object _lock = new();

    public MqttBrokerManager(ILogger<MqttBrokerManager> logger, IOptions<BrokerConfiguration> options)
    {
        _logger = logger;
        _configuration = options.Value;
    }

    public BrokerStatus GetStatus()
    {
        lock (_lock)
        {
            return new BrokerStatus
            {
                IsRunning = _mqttServer?.IsStarted ?? false,
                StartedAt = _startedAt,
                ConnectedClients = _connectedClientCount,
                Port = _configuration.Port
            };
        }
    }

    public async Task<IEnumerable<ConnectedClient>> GetConnectedClientsAsync()
    {
        if (_mqttServer == null || !_mqttServer.IsStarted)
        {
            return Enumerable.Empty<ConnectedClient>();
        }

        var clients = await _mqttServer.GetClientsAsync();
        return clients.Select(c => new ConnectedClient
        {
            ClientId = c.Id,
            Endpoint = c.RemoteEndPoint?.ToString(),
            ConnectedAt = c.ConnectedTimestamp,
            ProtocolVersion = c.ProtocolVersion.ToString()
        });
    }

    public BrokerConfiguration GetConfiguration()
    {
        lock (_lock)
        {
            return new BrokerConfiguration
            {
                Port = _configuration.Port,
                MaxPendingConnections = _configuration.MaxPendingConnections,
                EnableAuthentication = _configuration.EnableAuthentication,
                EnableVerboseLogging = _configuration.EnableVerboseLogging,
                CommunicationTimeout = _configuration.CommunicationTimeout
            };
        }
    }

    public void UpdateConfiguration(BrokerConfiguration configuration)
    {
        lock (_lock)
        {
            _configuration = configuration;
            _logger.LogInformation("Broker configuration updated. Port: {Port}", configuration.Port);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_mqttServer?.IsStarted == true)
            {
                _logger.LogWarning("MQTT broker is already running");
                return;
            }
        }

        _logger.LogInformation("Starting MQTT broker on port {Port}...", _configuration.Port);

        var optionsBuilder = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(_configuration.Port)
            .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(_configuration.CommunicationTimeout));

        var mqttFactory = new MqttServerFactory();
        _mqttServer = mqttFactory.CreateMqttServer(optionsBuilder.Build());

        // Set up event handlers
        _mqttServer.ClientConnectedAsync += OnClientConnectedAsync;
        _mqttServer.ClientDisconnectedAsync += OnClientDisconnectedAsync;
        _mqttServer.InterceptingPublishAsync += OnInterceptingPublishAsync;
        
        if (_configuration.EnableAuthentication)
        {
            _mqttServer.ValidatingConnectionAsync += OnValidatingConnectionAsync;
        }

        await _mqttServer.StartAsync();

        lock (_lock)
        {
            _startedAt = DateTime.UtcNow;
            _connectedClientCount = 0;
        }

        _logger.LogInformation("MQTT broker started successfully on port {Port}", _configuration.Port);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_mqttServer == null || !_mqttServer.IsStarted)
        {
            _logger.LogWarning("MQTT broker is not running");
            return;
        }

        _logger.LogInformation("Stopping MQTT broker...");

        await _mqttServer.StopAsync();

        lock (_lock)
        {
            _startedAt = null;
        }

        _logger.LogInformation("MQTT broker stopped successfully");
    }

    public async Task DisconnectClientAsync(string clientId)
    {
        if (_mqttServer == null || !_mqttServer.IsStarted)
        {
            throw new InvalidOperationException("MQTT broker is not running");
        }

        await _mqttServer.DisconnectClientAsync(clientId, MqttDisconnectReasonCode.NormalDisconnection);
        _logger.LogInformation("Client {ClientId} disconnected by administrator", clientId);
    }

    private Task OnClientConnectedAsync(ClientConnectedEventArgs args)
    {
        Interlocked.Increment(ref _connectedClientCount);
        _logger.LogInformation(
            "Client connected: {ClientId} from {Endpoint} using protocol {ProtocolVersion}",
            args.ClientId,
            args.RemoteEndPoint,
            args.ProtocolVersion);
        return Task.CompletedTask;
    }

    private Task OnClientDisconnectedAsync(ClientDisconnectedEventArgs args)
    {
        Interlocked.Decrement(ref _connectedClientCount);
        _logger.LogInformation(
            "Client disconnected: {ClientId}, Reason: {Reason}",
            args.ClientId,
            args.DisconnectType);
        return Task.CompletedTask;
    }

    private Task OnInterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
        if (_configuration.EnableVerboseLogging)
        {
            _logger.LogDebug(
                "Message published by {ClientId} to topic {Topic}",
                args.ClientId,
                args.ApplicationMessage.Topic);
        }
        return Task.CompletedTask;
    }

    private Task OnValidatingConnectionAsync(ValidatingConnectionEventArgs args)
    {
        // Basic authentication - in production, this should validate against a user store
        if (string.IsNullOrEmpty(args.UserName) || string.IsNullOrEmpty(args.Password))
        {
            args.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            _logger.LogWarning("Client {ClientId} authentication failed - missing credentials", args.ClientId);
        }
        else
        {
            _logger.LogInformation("Client {ClientId} authenticated successfully", args.ClientId);
        }
        return Task.CompletedTask;
    }
}
