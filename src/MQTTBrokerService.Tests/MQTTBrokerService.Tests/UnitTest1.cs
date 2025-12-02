using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MQTTBrokerService.Models;
using MQTTBrokerService.Services;

namespace MQTTBrokerService.Tests;

public class BrokerConfigurationTests
{
    [Fact]
    public void BrokerConfiguration_HasDefaultValues()
    {
        var config = new BrokerConfiguration();

        Assert.Equal(1883, config.Port);
        Assert.Equal(100, config.MaxPendingConnections);
        Assert.False(config.EnableAuthentication);
        Assert.False(config.EnableVerboseLogging);
        Assert.Equal(30, config.CommunicationTimeout);
    }

    [Fact]
    public void BrokerConfiguration_CanBeModified()
    {
        var config = new BrokerConfiguration
        {
            Port = 8883,
            MaxPendingConnections = 500,
            EnableAuthentication = true,
            EnableVerboseLogging = true,
            CommunicationTimeout = 60
        };

        Assert.Equal(8883, config.Port);
        Assert.Equal(500, config.MaxPendingConnections);
        Assert.True(config.EnableAuthentication);
        Assert.True(config.EnableVerboseLogging);
        Assert.Equal(60, config.CommunicationTimeout);
    }
}

public class BrokerStatusTests
{
    [Fact]
    public void BrokerStatus_Uptime_ReturnsNull_WhenNotStarted()
    {
        var status = new BrokerStatus
        {
            IsRunning = false,
            StartedAt = null
        };

        Assert.Null(status.Uptime);
    }

    [Fact]
    public void BrokerStatus_Uptime_ReturnsTimeSpan_WhenStarted()
    {
        var status = new BrokerStatus
        {
            IsRunning = true,
            StartedAt = DateTime.UtcNow.AddHours(-1)
        };

        Assert.NotNull(status.Uptime);
        Assert.True(status.Uptime.Value.TotalMinutes >= 59);
    }
}

public class MqttBrokerManagerTests
{
    private readonly Mock<ILogger<MqttBrokerManager>> _loggerMock;
    private readonly IOptions<BrokerConfiguration> _options;

    public MqttBrokerManagerTests()
    {
        _loggerMock = new Mock<ILogger<MqttBrokerManager>>();
        _options = Options.Create(new BrokerConfiguration
        {
            Port = 1883,
            MaxPendingConnections = 100,
            EnableAuthentication = false,
            EnableVerboseLogging = false,
            CommunicationTimeout = 30
        });
    }

    [Fact]
    public void GetStatus_ReturnsNotRunning_WhenBrokerNotStarted()
    {
        var manager = new MqttBrokerManager(_loggerMock.Object, _options);

        var status = manager.GetStatus();

        Assert.False(status.IsRunning);
        Assert.Null(status.StartedAt);
        Assert.Equal(0, status.ConnectedClients);
    }

    [Fact]
    public void GetConfiguration_ReturnsCurrentConfiguration()
    {
        var manager = new MqttBrokerManager(_loggerMock.Object, _options);

        var config = manager.GetConfiguration();

        Assert.Equal(1883, config.Port);
        Assert.Equal(100, config.MaxPendingConnections);
        Assert.False(config.EnableAuthentication);
    }

    [Fact]
    public void UpdateConfiguration_UpdatesSettings()
    {
        var manager = new MqttBrokerManager(_loggerMock.Object, _options);
        var newConfig = new BrokerConfiguration
        {
            Port = 8883,
            MaxPendingConnections = 500,
            EnableAuthentication = true,
            EnableVerboseLogging = true,
            CommunicationTimeout = 60
        };

        manager.UpdateConfiguration(newConfig);
        var config = manager.GetConfiguration();

        Assert.Equal(8883, config.Port);
        Assert.Equal(500, config.MaxPendingConnections);
        Assert.True(config.EnableAuthentication);
        Assert.True(config.EnableVerboseLogging);
        Assert.Equal(60, config.CommunicationTimeout);
    }

    [Fact]
    public async Task GetConnectedClientsAsync_ReturnsEmptyList_WhenBrokerNotStarted()
    {
        var manager = new MqttBrokerManager(_loggerMock.Object, _options);

        var clients = await manager.GetConnectedClientsAsync();

        Assert.Empty(clients);
    }
}

public class ConnectedClientTests
{
    [Fact]
    public void ConnectedClient_CanBeCreated()
    {
        var client = new ConnectedClient
        {
            ClientId = "test-client",
            Endpoint = "127.0.0.1:12345",
            ConnectedAt = DateTime.UtcNow,
            ProtocolVersion = "V500"
        };

        Assert.Equal("test-client", client.ClientId);
        Assert.Equal("127.0.0.1:12345", client.Endpoint);
        Assert.Equal("V500", client.ProtocolVersion);
    }
}
