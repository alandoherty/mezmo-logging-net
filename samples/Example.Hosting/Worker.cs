using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.Hosting;

/// <summary>
/// Implements a basic background service which logs a bit until told to stop.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    
    /// <summary>
    /// Run the service.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            _logger.LogInformation("This is some information being logged");

            using (_logger.BeginScope(new {IpAddress = IPAddress.Loopback.ToString()})) {
                try {
                    throw new ArgumentException("The argument is not without merit");
                } catch (Exception ex) {
                    _logger.LogError(ex, "This is some information being logged");
                }
            }

            _logger.LogWarning("An error just occured, something might be wrong!");

            try {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            } catch (OperationCanceledException) {
                break;
            }
        }
        
        _logger.LogInformation("Cancellation of service requested, shutting down");
    }

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }
}