using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<FolderMonitorService>();
    })
    .ConfigureLogging(logging => logging.AddConsole())
    .Build()
    .Run();
