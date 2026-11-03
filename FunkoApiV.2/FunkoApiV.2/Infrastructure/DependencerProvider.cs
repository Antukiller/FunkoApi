using FunkoApiV._2.Infrastructure.Interfaces;
using FunkoApiV._2.Service;
using FunkoApiV._2.Service.Proveedor;

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
        );

        services.Decorate<IFunkoService, CachedFunkoService>();
        services.Decorate<ICategoriaService, CachedCategoriaService>();
        services.Decorate<IProveedorService, CachedProveedorService>();

        return services;
    }
}
