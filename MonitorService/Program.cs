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
            services.AddHostedService<FileWatcherService>();
        })
        .UseSerilog();

    var host = builder.Build();

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
