using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorService.Services;

/// <summary>
/// Windows Service que monitora pasta e registra eventos em arquivo .log
/// Requisito C#-a: Monitorar criação e deleção de arquivos
/// </summary>
public class FileWatcherService : BackgroundService
{
    private readonly ILogger<FileWatcherService> _logger;
    private readonly IConfiguration _configuration;
    private FileSystemWatcher? _watcher;
    private readonly string _monitorPath;
    private readonly string _logPath;

    public FileWatcherService(
        ILogger<FileWatcherService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Lê configurações ou usa valores padrão
        _monitorPath = _configuration["FileWatcher:MonitorPath"] ?? @"C:\Integration\Drop";
        _logPath = _configuration["FileWatcher:LogPath"] ?? @"Logs";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Garante que a pasta monitorada existe
            Directory.CreateDirectory(_monitorPath);
            _logger.LogInformation("Monitoring folder created/verified: {Path}", _monitorPath);

            // Garante que a pasta de logs existe
            Directory.CreateDirectory(_logPath);

            // Configura o FileSystemWatcher
            _watcher = new FileSystemWatcher(_monitorPath)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };

            // Registra eventos de criação
            _watcher.Created += OnFileCreated;

            // Registra eventos de deleção
            _watcher.Deleted += OnFileDeleted;

            // Registra eventos de renomeação (opcional)
            _watcher.Renamed += OnFileRenamed;

            // Registra eventos de alteração (opcional)
            _watcher.Changed += OnFileChanged;

            // Registra erros
            _watcher.Error += OnError;

            _logger.LogInformation(
                "FileWatcher Service started successfully. Monitoring: {Path}",
                _monitorPath);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting FileWatcher Service");
            throw;
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            var fileInfo = new FileInfo(e.FullPath);
            var logMessage = $"[CREATED] File: {e.Name} | Size: {fileInfo.Length} bytes | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            _logger.LogInformation(logMessage);
            WriteToLogFile(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Created event for file: {FileName}", e.Name);
        }
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        try
        {
            var logMessage = $"[DELETED] File: {e.Name} | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            _logger.LogInformation(logMessage);
            WriteToLogFile(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Deleted event for file: {FileName}", e.Name);
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            var logMessage = $"[RENAMED] From: {e.OldName} → To: {e.Name} | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            _logger.LogInformation(logMessage);
            WriteToLogFile(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Renamed event for file: {FileName}", e.Name);
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var logMessage = $"[CHANGED] File: {e.Name} | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            _logger.LogInformation(logMessage);
            WriteToLogFile(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Changed event for file: {FileName}", e.Name);
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        var exception = e.GetException();
        _logger.LogError(exception, "FileSystemWatcher error occurred");
    }

    /// <summary>
    /// Escreve eventos em arquivo .log no formato especificado
    /// </summary>
    private void WriteToLogFile(string message)
    {
        try
        {
            var logFileName = $"file-monitor-{DateTime.Now:yyyy-MM-dd}.log";
            var logFilePath = Path.Combine(_logPath, logFileName);

            // Usa lock para thread-safety
            lock (this)
            {
                File.AppendAllText(logFilePath, message + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to log file");
        }
    }

    public override void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.Created -= OnFileCreated;
            _watcher.Deleted -= OnFileDeleted;
            _watcher.Renamed -= OnFileRenamed;
            _watcher.Changed -= OnFileChanged;
            _watcher.Error -= OnError;
            _watcher.Dispose();
        }

        _logger.LogInformation("FileWatcher Service disposed");
        base.Dispose();
    }
}

