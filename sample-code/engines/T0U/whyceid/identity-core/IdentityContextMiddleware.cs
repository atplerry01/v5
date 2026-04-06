using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Middleware that enriches CommandContext with identity claims.
/// Extracts identity metadata from command headers and stores them
/// as typed properties for downstream middleware and engines.
///
/// Pipeline position: AFTER Tracing, BEFORE Authorization.
/// </summary>
public sealed class IdentityContextMiddleware : IMiddleware
{
    public async Task<CommandResult> InvokeAsync(
        CommandContext context,
        MiddlewareDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        var headers = context.Envelope.Metadata.Headers;

        // Extract identity claims from headers into context properties
        if (headers.TryGetValue("X-WhyceId", out var identityId) && !string.IsNullOrEmpty(identityId))
            context.Set(IdentityContextKeys.IdentityId, identityId);

        if (headers.TryGetValue("X-Identity-Type", out var identityType))
            context.Set(IdentityContextKeys.IdentityType, identityType);

        if (headers.TryGetValue("X-Session-Id", out var sessionId))
            context.Set(IdentityContextKeys.SessionId, sessionId);

        if (headers.TryGetValue("X-Device-Id", out var deviceId))
            context.Set(IdentityContextKeys.DeviceId, deviceId);

        if (headers.TryGetValue("X-Trust-Level", out var trustLevel))
            context.Set(IdentityContextKeys.TrustLevel, trustLevel);

        if (headers.TryGetValue("X-Auth-Method", out var authMethod))
            context.Set(IdentityContextKeys.AuthenticationMethod, authMethod);

        if (headers.TryGetValue("X-Is-Service", out var isService))
            context.Set(IdentityContextKeys.IsServiceIdentity, isService);

        // Governance context — required by IdentityPolicyEnforcer
        if (headers.TryGetValue("X-Governance-Approved", out var governanceApproved))
            context.Set("Governance.ProposalApproved", governanceApproved);

        if (headers.TryGetValue("X-Has-Consent", out var hasConsent))
            context.Set("Identity.HasConsent", hasConsent);

        if (headers.TryGetValue("X-Roles", out var roles))
            context.Set(IdentityContextKeys.Roles, roles);

        if (headers.TryGetValue("X-Permissions", out var permissions))
            context.Set(IdentityContextKeys.Permissions, permissions);

        return await next(context);
    }
}
