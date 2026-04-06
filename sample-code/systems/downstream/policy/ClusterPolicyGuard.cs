using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Systems.Downstream.Policy;

/// <summary>
/// Policy guard for cluster operational systems.
/// READ-ONLY: reads the PolicyDecision already computed by PolicyMiddleware.
/// Does NOT invoke policy evaluation — that happens ONCE in the runtime pipeline.
/// </summary>
public static class ClusterPolicyGuard
{
    public static PolicyGuardResult Enforce(IPolicyDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (!decision.IsAllowed)
        {
            return PolicyGuardResult.Denied(
                decision.Reason ?? "Cluster operation denied by policy");
        }

        return PolicyGuardResult.Allowed();
    }
}
