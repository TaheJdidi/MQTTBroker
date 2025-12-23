using Microsoft.AspNetCore.Mvc;
using MQTTBrokerService.Models;
using MQTTBrokerService.Services;

namespace MQTTBrokerService.Controllers;

/// <summary>
/// API controller for managing the MQTT broker.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BrokerController : ControllerBase
{
    private readonly IMqttBrokerManager _brokerManager;
    private readonly ILogger<BrokerController> _logger;

    public BrokerController(IMqttBrokerManager brokerManager, ILogger<BrokerController> logger)
    {
        _brokerManager = brokerManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current status of the MQTT broker.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(BrokerStatus), StatusCodes.Status200OK)]
    public ActionResult<BrokerStatus> GetStatus()
    {
        var status = _brokerManager.GetStatus();
        return Ok(status);
    }

    /// <summary>
    /// Gets the list of connected clients.
    /// </summary>
    [HttpGet("clients")]
    [ProducesResponseType(typeof(IEnumerable<ConnectedClient>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConnectedClient>>> GetClients()
    {
        var clients = await _brokerManager.GetConnectedClientsAsync();
        return Ok(clients);
    }

    /// <summary>
    /// Gets the current broker configuration.
    /// </summary>
    [HttpGet("configuration")]
    [ProducesResponseType(typeof(BrokerConfiguration), StatusCodes.Status200OK)]
    public ActionResult<BrokerConfiguration> GetConfiguration()
    {
        var configuration = _brokerManager.GetConfiguration();
        return Ok(configuration);
    }

    /// <summary>
    /// Updates the broker configuration.
    /// </summary>
    [HttpPut("configuration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult UpdateConfiguration([FromBody] BrokerConfiguration configuration)
    {
        if (configuration.Port < 1 || configuration.Port > 65535)
        {
            return BadRequest("Port must be between 1 and 65535");
        }

        _brokerManager.UpdateConfiguration(configuration);
        _logger.LogInformation("Broker configuration updated via API");
        return Ok(new { message = "Configuration updated successfully" });
    }

    /// <summary>
    /// Starts the MQTT broker.
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Start()
    {
        try
        {
            await _brokerManager.StartAsync();
            _logger.LogInformation("Broker started via API");
            return Ok(new { message = "Broker started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start broker");
            return StatusCode(500, new { error = "Failed to start broker", details = ex.Message });
        }
    }

    /// <summary>
    /// Stops the MQTT broker.
    /// </summary>
    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Stop()
    {
        try
        {
            await _brokerManager.StopAsync();
            _logger.LogInformation("Broker stopped via API");
            return Ok(new { message = "Broker stopped successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop broker");
            return StatusCode(500, new { error = "Failed to stop broker", details = ex.Message });
        }
    }

    /// <summary>
    /// Disconnects a specific client.
    /// </summary>
    [HttpPost("clients/{clientId}/disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DisconnectClient(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return BadRequest("Client ID is required");
        }

        try
        {
            await _brokerManager.DisconnectClientAsync(clientId);
            return Ok(new { message = $"Client {clientId} disconnected successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect client {ClientId}", clientId);
            return StatusCode(500, new { error = "Failed to disconnect client", details = ex.Message });
        }
    }
}
