using MonitorService.Models;

namespace MonitorService.Interfaces;

public interface IFileEventLogger
{
    Task LogEventAsync(FileEvent fileEvent, CancellationToken cancellationToken = default);
    Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);
    Task LogInfoAsync(string message, CancellationToken cancellationToken = default);
    Task LogWarningAsync(string message, CancellationToken cancellationToken = default);
}
