using FunkoApiV._2.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace FunkoApiV._2.Infrastructure;

public static class RepositoryConfig {
    public static IServiceCollection AddRepositories(this IServiceCollection service) {
        service.AddSingleton<IFunkoRepository, FunkoRepository>();
        return service;
    }
}