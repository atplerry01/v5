using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Systems.Upstream.WhyceId.Middleware;

/// <summary>
/// Policy guard for WhyceID identity operations.
/// READ-ONLY: reads the PolicyDecision already computed by PolicyMiddleware.
/// Does NOT invoke policy evaluation — that happens ONCE in the runtime pipeline.
/// </summary>
public static class IdentityPolicyGuard
{
    public static PolicyGuardResult Enforce(IPolicyDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (!decision.IsAllowed)
        {
            return PolicyGuardResult.Denied(
                decision.Reason ?? "Identity operation denied by policy");
        }

        return PolicyGuardResult.Allowed();
    }
}
