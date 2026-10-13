using FunkoApiV._2.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FunkoApiV._2.Infrastructure;

public static class DependencerProvider {
    public static IServiceCollection AddDependencies(this IServiceCollection services) {
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(c => c.AssignableTo<ITransientService>())
            .AsImplementedInterfaces()
            .WithTransientLifetime()
            .AddClasses(c => c.AssignableTo<IScopedService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo<ISingletonService>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
        return services;
    }
}