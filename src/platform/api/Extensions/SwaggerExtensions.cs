using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Whycespace.Platform.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddWhyceSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("operational", new OpenApiInfo
            {
                Title = "Operational API",
                Version = "v1",
                Description = "Phase 1 — Sandbox / Todo Execution Surface"
            });

            options.SwaggerDoc("infrastructure", new OpenApiInfo
            {
                Title = "Infrastructure API",
                Version = "v1",
                Description = "Platform Health & Diagnostics"
            });

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                return docName switch
                {
                    "operational" => apiDesc.GroupName == "operational.sandbox.todo",
                    "infrastructure" => apiDesc.GroupName == "platform.infrastructure.health",
                    _ => false
                };
            });

            options.TagActionsBy(api =>
                new[] { api.GroupName ?? "default" });
        });

        return services;
    }
}
