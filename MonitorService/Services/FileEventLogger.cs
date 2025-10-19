using MonitorService.Interfaces;
using MonitorService.Models;

namespace MonitorService.Services;

public class FileEventLogger(IConfigurationService configurationService, ILogger<FileEventLogger> logger) : IFileEventLogger
{
    private readonly IConfigurationService _configurationService = configurationService;
    private readonly ILogger<FileEventLogger> _logger = logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task LogEventAsync(FileEvent fileEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var logConfig = _configurationService.GetLoggingConfiguration();
            var logMessage = FormatLogMessage(fileEvent);
            
            await WriteToLogFileAsync(logMessage, logConfig, cancellationToken);
            _logger.LogInformation("File event logged: {EventType} - {FileName}", fileEvent.EventType, fileEvent.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging file event: {EventType} - {FileName}", fileEvent.EventType, fileEvent.FileName);
            throw;
        }
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var logConfig = _configurationService.GetLoggingConfiguration();
            var logMessage = $"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}";
            
            if (exception != null)
            {
                logMessage += $" | Exception: {exception.Message}";
            }

            await WriteToLogFileAsync(logMessage, logConfig, cancellationToken);
            _logger.LogError(exception, "Error logged: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging error message: {Message}", message);
            throw;
        }
    }

    public async Task LogInfoAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var logConfig = _configurationService.GetLoggingConfiguration();
            var logMessage = $"[INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}";
            
            await WriteToLogFileAsync(logMessage, logConfig, cancellationToken);
            _logger.LogInformation("Info logged: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging info message: {Message}", message);
            throw;
        }
    }

    public async Task LogWarningAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var logConfig = _configurationService.GetLoggingConfiguration();
            var logMessage = $"[WARNING] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}";
            
            await WriteToLogFileAsync(logMessage, logConfig, cancellationToken);
            _logger.LogWarning("Warning logged: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging warning message: {Message}", message);
            throw;
        }
    }

    private static string FormatLogMessage(FileEvent fileEvent)
    {
        return fileEvent.EventType switch
        {
            FileEventType.Created => $"[CREATED] File: {fileEvent.FileName} | Size: {fileEvent.FileSize} bytes | Time: {fileEvent.Timestamp:yyyy-MM-dd HH:mm:ss}",
            FileEventType.Deleted => $"[DELETED] File: {fileEvent.FileName} | Time: {fileEvent.Timestamp:yyyy-MM-dd HH:mm:ss}",
            FileEventType.Changed => $"[CHANGED] File: {fileEvent.FileName} | Time: {fileEvent.Timestamp:yyyy-MM-dd HH:mm:ss}",
            FileEventType.Renamed => $"[RENAMED] From: {fileEvent.OldName} → To: {fileEvent.NewName} | Time: {fileEvent.Timestamp:yyyy-MM-dd HH:mm:ss}",
            FileEventType.Error => $"[ERROR] File: {fileEvent.FileName} | Error: {fileEvent.ErrorMessage} | Time: {fileEvent.Timestamp:yyyy-MM-dd HH:mm:ss}",
            _ => $"[UNKNOWN] File: {fileEvent.FileName} | Time: {fileEvent.Timestamp:yyyy-MM-dd HH:mm:ss}"
        };
    }

    private async Task WriteToLogFileAsync(string message, LoggingConfiguration config, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var logFileName = config.LogFileName.Replace("{date}", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            var logFilePath = Path.Combine(config.LogPath, logFileName);

            await File.AppendAllTextAsync(logFilePath, message + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
