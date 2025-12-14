using Microsoft.OpenApi.Models;
using Shared.Common.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Infrastructure.Idempotency;

public class IdempotencyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasIdempotency = context.MethodInfo
            .GetCustomAttributes(true)
            .Any(a => a.GetType().Name == nameof(IdempotentAttribute));

        if (!hasIdempotency)
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Idempotency-Key",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Idempotency key for safely retrying requests",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            }
        });
    }
}