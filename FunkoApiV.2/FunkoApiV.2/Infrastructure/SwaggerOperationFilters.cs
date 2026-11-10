using FunkoApiV._2.Dto.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FunkoApiV._2.Infrastructure;

public sealed class AuthorizeOperationFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        var attributes = context.MethodInfo.GetCustomAttributes(true)
            .Concat(context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? []);
        if (!attributes.OfType<IAuthorizeData>().Any() ||
            attributes.OfType<AllowAnonymousAttribute>().Any())
            return;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });
        AddResponse(operation, "401", "No autenticado", context);
        AddResponse(operation, "403", "No autorizado para realizar esta operación", context);
    }

    private static void AddResponse(
        OpenApiOperation operation,
        string statusCode,
        string description,
        OperationFilterContext context) {
        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd(statusCode, new OpenApiResponse {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType> {
                ["application/json"] = new() {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ApiErrorResponse), context.SchemaRepository)
                }
            }
        });
    }
}

public sealed class ApiResponsesOperationFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        operation.Responses ??= new OpenApiResponses();
        operation.Responses.TryAdd("429", new OpenApiResponse {
            Description = "Se ha superado el límite de peticiones por IP.",
            Content = new Dictionary<string, OpenApiMediaType> {
                ["application/json"] = new() {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ApiErrorResponse), context.SchemaRepository)
                }
            }
        });
    }
}
