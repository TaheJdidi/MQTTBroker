namespace MQTTBrokerService.Models;

/// <summary>
/// Represents the current status of the MQTT broker.
/// </summary>
public class BrokerStatus
{
    /// <summary>
    /// Gets or sets whether the broker is currently running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the broker was started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of currently connected clients.
    /// </summary>
    public int ConnectedClients { get; set; }

    /// <summary>
    /// Gets or sets the port the broker is listening on.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the uptime of the broker.
    /// </summary>
    public TimeSpan? Uptime => StartedAt.HasValue ? DateTime.UtcNow - StartedAt.Value : null;
}
