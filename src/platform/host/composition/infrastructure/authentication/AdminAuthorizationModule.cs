using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Platform.Host.Authorization;
using Whycespace.Shared.Contracts.Runtime.Admin;

namespace Whycespace.Platform.Host.Composition.Infrastructure.Authentication;

/// <summary>
/// R4.B / R-ADMIN-SCOPE-01 — registers the <see cref="AdminScope.PolicyName"/>
/// authorization policy and the <see cref="AdminAuthorizationHandler"/> that
/// satisfies it. Must be invoked AFTER
/// <see cref="AuthenticationInfrastructureModule.AddAuthentication"/> so the
/// <c>ICallerIdentityAccessor</c> is already registered.
/// </summary>
public static class AdminAuthorizationModule
{
    public static IServiceCollection AddAdminAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(AdminScope.PolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new AdminScopeRequirement());
            });
        return services;
    }
}
