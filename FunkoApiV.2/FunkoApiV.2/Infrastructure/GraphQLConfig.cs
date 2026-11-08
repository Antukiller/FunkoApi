using FunkoApiV._2.GraphQL.Queries;
using HotChocolate.Execution.Configuration;

namespace FunkoApiV._2.Infrastructure;

public class GraphQLConfig {
    public static IRequestExecutorBuilder AddGraphQLConfig(
        this IServiceCollection services,
        IWebHostEnvironment environment) {
        
        return services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<FunkoQuery>()
            
}