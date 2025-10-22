using Serilog;
using Api.Extensions;
using Microsoft.EntityFrameworkCore;
using LegacyBridge.Infrastructure.Extensions;
using LegacyBridge.Api.Extensions;

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

    // Connection string
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        builder.Services.AddDatabase(connectionString);
        builder.Services.AddDatabaseHealthChecks(connectionString);
    }

    // Registrar repositórios
    builder.Services.AddRepositories();

    // Registrar serviços
    builder.Services.AddApplicationServices();

    // AutoMapper: registra profiles do assembly atual (genérico para evitar ambiguidade)
    builder.Services.AddAutoMapperProfiles();

    // Controllers, Swagger e CORS
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Legacy Bridge API",
            Version = "v1",
            Description = "API para modernização gradual VB6 → C# + PostgreSQL",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Legacy Bridge Team",
                Email = "support@legacybridge.com"
            }
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost" };
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

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
    app.UseMiddleware<Api.Middleware.ExceptionHandlingMiddleware>();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Endpoint de status
    app.MapGet("/", () => new
    {
        name = "Legacy Bridge API",
        version = "1.0",
        status = "Running",
        environment,
        endpoints = Endpoints()
    }).ExcludeFromDescription();

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

static string[] Endpoints() => new[]
{
    "/health",
    "/api/products",
    "/api/customers/search?term={searchTerm}",
    "/swagger"
};

// Classe Program pública para testes
public partial class Program { }