using MonitorService.Models;

namespace MonitorService.Interfaces;

public interface IFileWatcherService
{
    Task StartWatchingAsync(CancellationToken cancellationToken = default);
    Task StopWatchingAsync();
    bool IsWatching { get; }
    event EventHandler<FileEvent>? FileEventOccurred;
    event EventHandler<Exception>? ErrorOccurred;
}
