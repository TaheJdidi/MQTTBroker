namespace MQTTBrokerService.Models;

/// <summary>
/// Represents a connected MQTT client.
/// </summary>
public class ConnectedClient
{
    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client's endpoint (IP:Port).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the client connected.
    /// </summary>
    public DateTime ConnectedAt { get; set; }

    /// <summary>
    /// Gets or sets the MQTT protocol version used by the client.
    /// </summary>
    public string ProtocolVersion { get; set; } = string.Empty;
}
