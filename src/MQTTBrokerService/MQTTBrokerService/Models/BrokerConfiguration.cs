namespace MQTTBrokerService.Models;

/// <summary>
/// Configuration settings for the MQTT broker.
/// </summary>
public class BrokerConfiguration
{
    /// <summary>
    /// Gets or sets the port number for the MQTT broker. Default is 1883.
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// Gets or sets the maximum number of pending connections. Default is 100.
    /// </summary>
    public int MaxPendingConnections { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether authentication is enabled. Default is false.
    /// </summary>
    public bool EnableAuthentication { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable verbose logging. Default is false.
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the communication timeout in seconds. Default is 30.
    /// </summary>
    public int CommunicationTimeout { get; set; } = 30;
}
