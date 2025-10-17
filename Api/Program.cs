using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Legacy Bridge API (Environment: {Environment})", environment);

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args
    });

    builder.Configuration.AddConfiguration(configuration);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Legacy Bridge API",
            Version = "v1",
            Description = "API para modernização gradual VB6 → C# + PostgreSQL"
        });
    });

    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost" };

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var healthChecksBuilder = builder.Services.AddHealthChecks();
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        healthChecksBuilder.AddNpgSql(connectionString!, name: "PostgreSQL");
    }
    else
    {
        Log.Warning("Connection string 'DefaultConnection' não encontrada. Health check PostgreSQL não registrado.");
    }

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Legacy Bridge API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.MapGet("/", () => new
    {
        name = "Legacy Bridge API",
        version = "1.0",
        status = "Running",
        environment = environment,
        endpoints = new[]
        {
            "/health",
            "/api/products",
            "/api/customers/search?term={searchTerm}",
            "/swagger"
        }
    });

    Log.Information("API configured successfully. Listening on configured ports...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
