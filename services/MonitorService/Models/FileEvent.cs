namespace MonitorService.Models;

public class FileEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public FileEventType EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long? FileSize { get; set; }
    public string? OldName { get; set; }
    public string? NewName { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum FileEventType
{
    Created,
    Deleted,
    Changed,
    Renamed,
    Error
}

public class FileEventLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public FileEvent Event { get; set; } = new();
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public string LogMessage { get; set; } = string.Empty;
    public string LogLevel { get; set; } = "Information";
}
