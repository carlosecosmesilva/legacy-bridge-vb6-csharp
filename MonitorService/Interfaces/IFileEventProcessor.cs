using MonitorService.Models;

namespace MonitorService.Interfaces;

public interface IFileEventProcessor
{
    Task ProcessEventAsync(FileEvent fileEvent, CancellationToken cancellationToken = default);
    Task<bool> ShouldProcessEventAsync(FileEvent fileEvent, CancellationToken cancellationToken = default);
}
