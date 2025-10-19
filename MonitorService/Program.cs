using MonitorService.Interfaces;
using MonitorService.Services;
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Legacy Bridge Monitor Service");

    var builder = Host.CreateDefaultBuilder(args)
        .UseWindowsService(options =>
        {
            options.ServiceName = "LegacyBridgeMonitor";
        })
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddConfiguration(configuration);
        })
        .ConfigureServices((context, services) =>
        {
            // Register services with dependency injection
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IFileEventLogger, FileEventLogger>();
            services.AddSingleton<IFileEventProcessor, FileEventProcessor>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<IFileWatcherService, FileWatcherService>();
            
            services.AddHostedService<MonitorBackgroundService>();
        })
        .UseSerilog();

    var host = builder.Build();

    var configService = host.Services.GetRequiredService<IConfigurationService>();
    configService.ValidateConfiguration();

    Log.Information("Monitor Service configured successfully");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Monitor Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}