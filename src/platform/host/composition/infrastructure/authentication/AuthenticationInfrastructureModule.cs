using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Authentication;

/// <summary>
/// WP-1 (Security Binding Completion): Registers JWT Bearer authentication
/// and the caller identity accessor. Fail-closed configuration — missing
/// signing key throws at startup. No fallback to anonymous execution.
///
/// Configuration:
///   Jwt:Issuer           — token issuer (appsettings.json, non-secret)
///   Jwt:Audience          — token audience (appsettings.json, non-secret)
///   Jwt:SigningKey         — HMAC-SHA256 key (environment variable, secret)
///   Jwt:ClockSkewSeconds  — tolerance for expiry checks (default 30)
///
/// Determinism note: JWT expiry validation uses the framework's internal
/// clock at the HTTP boundary. This is a platform-edge concern and does not
/// violate domain/runtime determinism (GE-01). Domain identity resolution
/// occurs downstream in PolicyMiddleware via WhyceIdEngine.
/// </summary>
public static class AuthenticationInfrastructureModule
{
    public static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var signingKey = configuration.GetValue<string>("Jwt:SigningKey");
        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: Jwt:SigningKey is required. " +
                "Set the JWT__SigningKey environment variable. " +
                "No fallback — unauthenticated execution is not permitted.");

        var issuer = configuration.GetValue<string>("Jwt:Issuer") ?? "whycespace";
        var audience = configuration.GetValue<string>("Jwt:Audience") ?? "whycespace-api";
        var clockSkewSeconds = configuration.GetValue<int?>("Jwt:ClockSkewSeconds") ?? 30;

        services.AddHttpContextAccessor();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(signingKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(clockSkewSeconds),

                // Require 'sub' claim for actor identification
                NameClaimType = "sub",
                RoleClaimType = "roles",
            };

            // Fail-closed: no automatic redirect, return 401 directly
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(
                        """{"error":"WP-1: Authentication required. Provide a valid JWT Bearer token."}""");
                },
                OnForbidden = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(
                        """{"error":"WP-1: Access denied. Insufficient permissions."}""");
                }
            };
        });

        services.AddAuthorization();

        // Caller identity accessor — singleton, reads per-request context via
        // IHttpContextAccessor async-local. Consumed by SystemIntentDispatcher.
        services.AddSingleton<ICallerIdentityAccessor, HttpCallerIdentityAccessor>();

        return services;
    }
}
