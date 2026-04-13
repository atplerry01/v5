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

            // WP-1: JWT Bearer authentication support in Swagger UI.
            // Enables the "Authorize" button for testing authenticated endpoints.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter your JWT token. Operational endpoints require authentication.",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
            });

            // OpenApi v2 API: AddSecurityRequirement takes a factory that
            // receives the document and returns a requirement referencing
            // the scheme by name via OpenApiSecuritySchemeReference.
            options.AddSecurityRequirement(doc =>
            {
                var requirement = new OpenApiSecurityRequirement();
                var schemeRef = new OpenApiSecuritySchemeReference("Bearer", doc);
                requirement.Add(schemeRef, new List<string>());
                return requirement;
            });

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                return docName switch
                {
                    "operational" => apiDesc.GroupName == "operational.sandbox.todo"
                        || apiDesc.GroupName == "operational.sandbox.kanban",
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
