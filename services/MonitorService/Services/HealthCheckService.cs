using MonitorService.Interfaces;
using MonitorService.Models;

namespace MonitorService.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly Dictionary<string, Func<Task<bool>>> _healthChecks = new();

    public HealthCheckService(ILogger<HealthCheckService> logger)
    {
        _logger = logger;

        RegisterHealthCheck("FileSystem", CheckFileSystemHealthAsync);
        RegisterHealthCheck("Memory", CheckMemoryHealthAsync);
    }

    public async Task<ServiceHealth> GetHealthAsync()
    {
        var health = new ServiceHealth
        {
            LastCheck = DateTime.UtcNow
        };

        var tasks = _healthChecks.Select(async kvp =>
        {
            var startTime = DateTime.UtcNow;
            try
            {
                var isHealthy = await kvp.Value();
                var duration = DateTime.UtcNow - startTime;

                return new HealthCheck
                {
                    Name = kvp.Key,
                    IsHealthy = isHealthy,
                    Status = isHealthy ? "Healthy" : "Unhealthy",
                    Duration = duration
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Health check failed: {CheckName}", kvp.Key);

                return new HealthCheck
                {
                    Name = kvp.Key,
                    IsHealthy = false,
                    Status = "Error",
                    Duration = duration,
                    ErrorMessage = ex.Message
                };
            }
        });

        var results = await Task.WhenAll(tasks);
        health.Checks = [.. results];
        health.IsHealthy = results.All(check => check.IsHealthy);
        health.Status = health.IsHealthy ? "Healthy" : "Unhealthy";

        if (!health.IsHealthy)
        {
            var failedChecks = results.Where(check => !check.IsHealthy).ToList();
            health.ErrorMessage = $"Failed checks: {string.Join(", ", failedChecks.Select(c => c.Name))}";
        }

        return health;
    }

    public async Task<bool> IsHealthyAsync()
    {
        var health = await GetHealthAsync();
        return health.IsHealthy;
    }

    public void RegisterHealthCheck(string name, Func<Task<bool>> healthCheck)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Health check name cannot be null or empty", nameof(name));
        }

        if (healthCheck != null)
        {
            _healthChecks[name] = healthCheck;
            _logger.LogInformation("Registered health check: {CheckName}", name);
        }
        else
        {
            throw new ArgumentNullException(nameof(healthCheck));
        }
    }


    private async Task<bool> CheckFileSystemHealthAsync()
    {
        try
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logPath))  
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));
          
            var testFile = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Logs"), $"health-check-{Guid.NewGuid()}.tmp");
            await File.WriteAllTextAsync(testFile, "health check");
            File.Delete(testFile);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File system health check failed");
            return false;
        }
    }

    private async Task<bool> CheckMemoryHealthAsync()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryUsageMB = process.WorkingSet64 / 1024 / 1024;
            var isHealthy = memoryUsageMB < 500;

            if (!isHealthy)
            {
                _logger.LogWarning("High memory usage detected: {MemoryUsageMB}MB", memoryUsageMB);
            }

            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return false;
        }
    }
}
