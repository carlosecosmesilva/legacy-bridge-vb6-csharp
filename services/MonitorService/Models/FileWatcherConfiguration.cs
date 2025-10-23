namespace MonitorService.Models;

public class FileWatcherConfiguration
{
    public string MonitorPath { get; set; } = string.Empty;
    public string LogPath { get; set; } = "Logs";
    public bool IncludeSubdirectories { get; set; } = false;
    public string FileFilters { get; set; } = "*.*";
    public bool EnableRaisingEvents { get; set; } = true;
    public NotifyFilters NotifyFilters { get; set; } = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
    public int BufferSize { get; set; } = 8192;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}

public class LoggingConfiguration
{
    public string LogFileName { get; set; } = "file-monitor-{date}.log";
    public string LogFormat { get; set; } = "[{level}] {timestamp:yyyy-MM-dd HH:mm:ss} - {message}";
    public string LogPath { get; set; } = "Logs";
    public int MaxLogFileSizeMB { get; set; } = 10;
    public int MaxLogFiles { get; set; } = 30;
    public bool EnableStructuredLogging { get; set; } = true;
}
