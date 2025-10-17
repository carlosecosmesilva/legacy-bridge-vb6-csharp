using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FolderMonitorService : BackgroundService
{
    private readonly ILogger<FolderMonitorService> _logger;
    private FileSystemWatcher? _watcher;
    private readonly string _path = @"C:\Integration\Drop";

    public FolderMonitorService(ILogger<FolderMonitorService> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_path);
        _watcher = new FileSystemWatcher(_path)
        {
            EnableRaisingEvents = true
        };
        _watcher.Created += (s, e) => _logger.LogInformation($"File created: {e.Name}");
        _watcher.Deleted += (s, e) => _logger.LogInformation($"File deleted: {e.Name}");
        return Task.CompletedTask;
    }
}
