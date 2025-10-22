using MonitorService.Interfaces;
using MonitorService.Models;

namespace MonitorService.Services;

public class FileWatcherService(
    IConfigurationService configurationService,
    IFileEventProcessor eventProcessor,
    ILogger<FileWatcherService> logger) : IFileWatcherService, IDisposable
{
    private readonly IConfigurationService _configurationService = configurationService;
    private readonly IFileEventProcessor _eventProcessor = eventProcessor;
    private readonly ILogger<FileWatcherService> _logger = logger;
    private FileSystemWatcher? _watcher;
    private bool _isWatching;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public event EventHandler<FileEvent>? FileEventOccurred;
    public event EventHandler<Exception>? ErrorOccurred;

    public bool IsWatching => _isWatching;

    public async Task StartWatchingAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_isWatching)
            {
                _logger.LogWarning("FileWatcher is already running");
                return;
            }

            var config = _configurationService.GetFileWatcherConfiguration();
            
            // Valida e criar diretório se necessário
            if (!Directory.Exists(config.MonitorPath))
            {
                Directory.CreateDirectory(config.MonitorPath);
                _logger.LogInformation("Created monitor directory: {Path}", config.MonitorPath);
            }

            // Configura FileSystemWatcher
            _watcher = new FileSystemWatcher(config.MonitorPath)
            {
                EnableRaisingEvents = config.EnableRaisingEvents,
                IncludeSubdirectories = config.IncludeSubdirectories,
                NotifyFilter = config.NotifyFilters,
                Filter = config.FileFilters,
                InternalBufferSize = config.BufferSize
            };

            // Registra eventos
            _watcher.Created += OnFileCreated;
            _watcher.Deleted += OnFileDeleted;
            _watcher.Changed += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;
            _watcher.Error += OnError;

            _isWatching = true;
            _logger.LogInformation("FileWatcher started successfully. Monitoring: {Path}", config.MonitorPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting FileWatcher");
            OnErrorOccurred(ex);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StopWatchingAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!_isWatching)
            {
                _logger.LogWarning("FileWatcher is not running");
                return;
            }

            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileCreated;
                _watcher.Deleted -= OnFileDeleted;
                _watcher.Changed -= OnFileChanged;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Error -= OnError;
                _watcher.Dispose();
                _watcher = null;
            }

            _isWatching = false;
            _logger.LogInformation("FileWatcher stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping FileWatcher");
            OnErrorOccurred(ex);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e) => await ProcessFileEventAsync(FileEventType.Created, e);

    private async void OnFileDeleted(object sender, FileSystemEventArgs e) => await ProcessFileEventAsync(FileEventType.Deleted, e);

    private async void OnFileChanged(object sender, FileSystemEventArgs e) => await ProcessFileEventAsync(FileEventType.Changed, e);
    

    private async void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            var fileEvent = new FileEvent
            {
                FileName = e.Name,
                FullPath = e.FullPath,
                EventType = FileEventType.Renamed,
                Timestamp = DateTime.UtcNow,
                OldName = e.OldName,
                NewName = e.Name
            };

            // Verifica se deve processar o evento
            try
            {
                if (File.Exists(e.FullPath))
                {
                    var fileInfo = new FileInfo(e.FullPath);
                    fileEvent.FileSize = fileInfo.Length;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get file size for renamed file: {FileName}", e.Name);
            }

            await ProcessEventAsync(fileEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing renamed file: {FileName}", e.Name);
            OnErrorOccurred(ex);
        }
    }

    private async void OnError(object sender, ErrorEventArgs e)
    {
        var exception = e.GetException();
        _logger.LogError(exception, "FileSystemWatcher error occurred");
        
        var fileEvent = new FileEvent
        {
            EventType = FileEventType.Error,
            Timestamp = DateTime.UtcNow,
            ErrorMessage = exception.Message
        };

        await ProcessEventAsync(fileEvent);
        OnErrorOccurred(exception);
    }

    private async Task ProcessFileEventAsync(FileEventType eventType, FileSystemEventArgs e)
    {
        try
        {
            var fileEvent = new FileEvent
            {
                FileName = e.Name,
                FullPath = e.FullPath,
                EventType = eventType,
                Timestamp = DateTime.UtcNow
            };

            // Verifica se deve processar o evento
            if (eventType == FileEventType.Created || eventType == FileEventType.Changed)
            {
                try
                {
                    if (File.Exists(e.FullPath))
                    {
                        var fileInfo = new FileInfo(e.FullPath);
                        fileEvent.FileSize = fileInfo.Length;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get file size for file: {FileName}", e.Name);
                }
            }

            await ProcessEventAsync(fileEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file event: {EventType} - {FileName}", eventType, e.Name);
            OnErrorOccurred(ex);
        }
    }

    private async Task ProcessEventAsync(FileEvent fileEvent)
    {
        try
        {
            // Verifica se deve processar o evento
            var shouldProcess = await _eventProcessor.ShouldProcessEventAsync(fileEvent);
            if (!shouldProcess)
            {
                _logger.LogDebug("Skipping file event: {EventType} - {FileName}", fileEvent.EventType, fileEvent.FileName);
                return;
            }

            // Processa o evento
            await _eventProcessor.ProcessEventAsync(fileEvent);
            
            // Notifica sobre o evento
            OnFileEventOccurred(fileEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file event: {EventType} - {FileName}", fileEvent.EventType, fileEvent.FileName);
            OnErrorOccurred(ex);
        }
    }

    private void OnFileEventOccurred(FileEvent fileEvent)
    {
        FileEventOccurred?.Invoke(this, fileEvent);
    }

    private void OnErrorOccurred(Exception exception)
    {
        ErrorOccurred?.Invoke(this, exception);
    }

    public void Dispose()
    {
        StopWatchingAsync().GetAwaiter().GetResult();
        _semaphore?.Dispose();
        _watcher?.Dispose();
    }
}
