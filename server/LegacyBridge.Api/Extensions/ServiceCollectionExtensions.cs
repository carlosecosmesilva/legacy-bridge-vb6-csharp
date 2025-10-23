using LegacyBridge.Application.Interfaces;
using LegacyBridge.Application.Services;
using LegacyBridge.Domain.Interfaces.Repositories;
using LegacyBridge.Infrastructure.Repositories;

namespace LegacyBridge.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}

