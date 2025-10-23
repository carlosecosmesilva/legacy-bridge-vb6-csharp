using MonitorService.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MonitorService.Services;

public class MonitorBackgroundService(
    IFileWatcherService fileWatcherService,
    IHealthCheckService healthCheckService,
    ILogger<MonitorBackgroundService> logger) : BackgroundService
{
    private readonly IFileWatcherService _fileWatcherService = fileWatcherService;
    private readonly IHealthCheckService _healthCheckService = healthCheckService;
    private readonly ILogger<MonitorBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Monitor Background Service starting...");

            // Registra eventos do FileWatcher
            _fileWatcherService.FileEventOccurred += OnFileEventOccurred;
            _fileWatcherService.ErrorOccurred += OnErrorOccurred;

            // Inicia monitoramento
            await _fileWatcherService.StartWatchingAsync(stoppingToken);

            _logger.LogInformation("Monitor Background Service started successfully");

            // Mantém o serviço rodando
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Verifica saúde do serviço periodicamente
                    var isHealthy = await _healthCheckService.IsHealthyAsync();
                    if (!isHealthy)
                    {
                        _logger.LogWarning("Service health check failed");
                    }

                    // Aguarda antes da próxima verificação
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background service loop");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Monitor Background Service failed to start");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Monitor Background Service stopping...");

            // Para o FileWatcher
            await _fileWatcherService.StopWatchingAsync();

            // Desregistra eventos
            _fileWatcherService.FileEventOccurred -= OnFileEventOccurred;
            _fileWatcherService.ErrorOccurred -= OnErrorOccurred;

            _logger.LogInformation("Monitor Background Service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Monitor Background Service");
        }
        finally
        {
            await base.StopAsync(cancellationToken);
        }
    }

    private void OnFileEventOccurred(object? sender, Models.FileEvent fileEvent)
    {
        _logger.LogDebug("File event occurred: {EventType} - {FileName}", 
            fileEvent.EventType, fileEvent.FileName);
    }

    private void OnErrorOccurred(object? sender, Exception exception)
    {
        _logger.LogError(exception, "FileWatcher error occurred");
    }
}
