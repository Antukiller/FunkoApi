using FunkoApiV._2.DataLoaders;
using FunkoApiV._2.GraphQL.Mutation;
using FunkoApiV._2.GraphQL.Queries;
using FunkoApiV._2.GraphQL.Resolvers;
using FunkoApiV._2.GraphQL.Subscriptions;
using FunkoApiV._2.GraphQL.Types;
using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;

namespace FunkoApiV._2.Infrastructure;

public static class GraphQLConfig {
    public static IRequestExecutorBuilder AddGraphQLConfig(
        this IServiceCollection services,
        IWebHostEnvironment environment) {

        return services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<FunkoQuery>()
            .AddMutationType<FunkoMutation>()
            .AddSubscriptionType<FunkoSubscriptions>()
            .AddInMemorySubscriptions()
            .AddUploadType()
            .AddType<FunkoType>()
            .AddType<CategoriaType>()
            .AddTypeExtension<FunkoResolvers>()
            .AddDataLoader<CategoriaByIdDataLoader>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .ModifyRequestOptions(opt =>
            {
                opt.IncludeExceptionDetails = environment.IsDevelopment();
            });
    }

    public static void MapGraphQLEndpoints(this WebApplication app) {
        app.MapGraphQL().WithOptions(options =>
            options.Tool.Enable = app.Environment.IsDevelopment());
        app.MapGraphQLWebSocket();
    }
}