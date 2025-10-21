using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Api.Mappings;

namespace Api.Extensions
{
    public static class MappingExtensions
    {
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            // Registra AutoMapper manualmente para evitar ambiguidade de AddAutoMapper em alguns ambientes
            services.AddSingleton(provider =>
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<CustomerProfile>();
                    cfg.AddProfile<ProductProfile>();
                });

                return config;
            });

            services.AddSingleton<IMapper>(sp =>
                sp.GetRequiredService<MapperConfiguration>().CreateMapper(sp.GetService));

            return services;
        }
    }
}
