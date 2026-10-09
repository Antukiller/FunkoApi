using FunkoApiV._2.Service;

namespace FunkoApiV._2.Infrastructure;

public static class ServiceConfig {
    public static IServiceCollection AddServices(this IServiceCollection services) {
        services.AddScoped<IFunkoService, FunkoService>();
        return services;
    }
}