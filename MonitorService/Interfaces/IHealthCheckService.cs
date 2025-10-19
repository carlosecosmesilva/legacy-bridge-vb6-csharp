using MonitorService.Models;

namespace MonitorService.Interfaces;

public interface IHealthCheckService
{
    Task<ServiceHealth> GetHealthAsync();
    Task<bool> IsHealthyAsync();
    void RegisterHealthCheck(string name, Func<Task<bool>> healthCheck);
}
