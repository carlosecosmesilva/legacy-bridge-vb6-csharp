namespace MonitorService.Models;

public class ServiceHealth
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    public List<HealthCheck> Checks { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

public class HealthCheck
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
}
