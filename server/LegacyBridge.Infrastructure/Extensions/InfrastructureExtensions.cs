using Microsoft.EntityFrameworkCore;
using LegacyBridge.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace LegacyBridge.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException(
                "Connection string cannot be null or empty",
                nameof(connectionString));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddDatabaseHealthChecks(this IServiceCollection services, string connectionString)
    {
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "PostgreSQL");
        return services;
    }
}
