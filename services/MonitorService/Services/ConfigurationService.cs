using MonitorService.Interfaces;
using MonitorService.Models;

namespace MonitorService.Services;

public class ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger) : IConfigurationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ConfigurationService> _logger = logger;

    public FileWatcherConfiguration GetFileWatcherConfiguration()
    {
        var config = new FileWatcherConfiguration();
        _configuration.GetSection("FileWatcher").Bind(config);
        
        if (string.IsNullOrWhiteSpace(config.MonitorPath))
        {
            config.MonitorPath = @"C:\Integration\Drop";
            _logger.LogWarning("MonitorPath not configured, using default: {Path}", config.MonitorPath);
        }

        if (string.IsNullOrWhiteSpace(config.LogPath))
        {
            config.LogPath = "Logs";
            _logger.LogWarning("LogPath not configured, using default: {Path}", config.LogPath);
        }

        return config;
    }

    public LoggingConfiguration GetLoggingConfiguration()
    {
        var config = new LoggingConfiguration();
        _configuration.GetSection("Logging").Bind(config);
        return config;
    }

    public T GetConfiguration<T>(string sectionName) where T : class, new()
    {
        var config = new T();
        _configuration.GetSection(sectionName).Bind(config);
        return config;
    }

    public void ValidateConfiguration()
    {
        var fileWatcherConfig = GetFileWatcherConfiguration();
        var loggingConfig = GetLoggingConfiguration();

        if (!Directory.Exists(fileWatcherConfig.MonitorPath))
        {
            try
            {
                Directory.CreateDirectory(fileWatcherConfig.MonitorPath);
                _logger.LogInformation("Created monitor directory: {Path}", fileWatcherConfig.MonitorPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create monitor directory: {Path}", fileWatcherConfig.MonitorPath);
                throw new InvalidOperationException($"Cannot create monitor directory: {fileWatcherConfig.MonitorPath}", ex);
            }
        }

        if (!Directory.Exists(loggingConfig.LogPath))
        {
            try
            {
                Directory.CreateDirectory(loggingConfig.LogPath);
                _logger.LogInformation("Created log directory: {Path}", loggingConfig.LogPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create log directory: {Path}", loggingConfig.LogPath);
                throw new InvalidOperationException($"Cannot create log directory: {loggingConfig.LogPath}", ex);
            }
        }

        _logger.LogInformation("Configuration validation completed successfully");
    }
}
