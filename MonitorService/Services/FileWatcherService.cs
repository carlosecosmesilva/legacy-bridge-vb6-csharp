using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FileWatcherService : BackgroundService
{
    private readonly ILogger<FileWatcherService> _logger;
    private FileSystemWatcher? _watcher;
    private readonly string _path = @"C:\Integration\Drop";

    public FileWatcherService(ILogger<FileWatcherService> logger)
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
