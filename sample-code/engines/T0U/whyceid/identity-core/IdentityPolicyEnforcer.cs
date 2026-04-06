using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Identity-specific policy enforcement middleware.
/// Extends the runtime pipeline with identity governance checks:
/// proposal requirements, sanction evaluation, consent gates.
///
/// Pipeline position: AFTER PolicyMiddleware, BEFORE ExecutionGuard.
/// ALL actions MUST pass policy; deny = STOP execution.
/// </summary>
public sealed class IdentityPolicyEnforcer : IMiddleware
{
    private static readonly HashSet<string> IdentityActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "identity.create", "identity.activate", "identity.suspend", "identity.deactivate",
        "credential.issue", "credential.revoke", "credential.rotate",
        "role.create", "role.assign", "role.revoke", "role.deactivate",
        "permission.create", "permission.grant", "permission.revoke", "permission.deactivate",
        "trust.initialize", "trust.record-factor", "trust.freeze", "trust.unfreeze",
        "verification.create", "verification.add-attempt",
        "consent.grant", "consent.revoke", "consent.expire",
        "session.start", "session.refresh", "session.revoke", "session.expire",
        "device.register", "device.verify", "device.block", "device.unblock", "device.deregister",
        "service-identity.register", "service-identity.suspend", "service-identity.decommission",
        "identity-graph.create", "identity-graph.link", "identity-graph.merge", "identity-graph.close",
        "access-profile.create", "access-profile.add-role", "access-profile.suspend"
    };

    private static readonly HashSet<string> ProposalRequired = new(StringComparer.OrdinalIgnoreCase)
    {
        "identity.deactivate", "role.create", "role.deactivate",
        "permission.create", "permission.deactivate",
        "service-identity.decommission", "identity-graph.merge",
        "identity-graph.close", "access-profile.suspend"
    };

    private static readonly HashSet<string> ConsentRequired = new(StringComparer.OrdinalIgnoreCase)
    {
        "credential.issue", "service-identity.register",
        "identity-graph.link", "device.register"
    };

    public async Task<CommandResult> InvokeAsync(
        CommandContext context,
        MiddlewareDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        var commandType = context.Envelope.CommandType;

        // Only enforce on identity commands
        if (!IdentityActions.Contains(commandType))
            return await next(context);

        // Check proposal requirement
        if (ProposalRequired.Contains(commandType))
        {
            var hasApproval = context.Get<string>("Governance.ProposalApproved");
            if (hasApproval is null || hasApproval != "true")
            {
                return CommandResult.Fail(
                    context.Envelope.CommandId,
                    $"Action '{commandType}' requires governance proposal and approval.",
                    "GOVERNANCE_PROPOSAL_REQUIRED");
            }
        }

        // Check consent requirement
        if (ConsentRequired.Contains(commandType))
        {
            var hasConsent = context.Get<string>("Identity.HasConsent");
            if (hasConsent is null || hasConsent != "true")
            {
                return CommandResult.Fail(
                    context.Envelope.CommandId,
                    $"Action '{commandType}' requires active consent.",
                    "CONSENT_REQUIRED");
            }
        }

        return await next(context);
    }
}
