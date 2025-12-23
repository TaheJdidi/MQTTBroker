using MQTTBrokerService.Services;

namespace MQTTBrokerService;

/// <summary>
/// Background service that manages the MQTT broker lifecycle.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttBrokerManager _brokerManager;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IMqttBrokerManager brokerManager, IConfiguration configuration)
    {
        _logger = logger;
        _brokerManager = brokerManager;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT Broker Service starting...");

        // Auto-start the broker if configured
        var autoStart = _configuration.GetValue<bool>("MqttBroker:AutoStart", true);
        if (autoStart)
        {
            try
            {
                await _brokerManager.StartAsync(stoppingToken);
                _logger.LogInformation("MQTT Broker auto-started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-start MQTT Broker");
            }
        }

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Broker Service stopping...");
        
        try
        {
            await _brokerManager.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MQTT Broker");
        }

        await base.StopAsync(cancellationToken);
    }
}
