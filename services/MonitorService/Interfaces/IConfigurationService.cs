using MonitorService.Models;

namespace MonitorService.Interfaces;

public interface IConfigurationService
{
    FileWatcherConfiguration GetFileWatcherConfiguration();
    LoggingConfiguration GetLoggingConfiguration();
    T GetConfiguration<T>(string sectionName) where T : class, new();
    void ValidateConfiguration();
}
