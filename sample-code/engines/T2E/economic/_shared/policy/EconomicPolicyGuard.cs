using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Economic.Shared.Policy;

/// <summary>
/// Policy guard for economic operations.
/// READ-ONLY: reads the PolicyDecision already computed by PolicyMiddleware.
/// Does NOT invoke policy evaluation — that happens ONCE in the runtime pipeline.
/// </summary>
public static class EconomicPolicyGuard
{
    public static PolicyGuardResult Enforce(IPolicyDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (!decision.IsAllowed)
        {
            return PolicyGuardResult.Denied(
                decision.Reason ?? "Economic operation denied by policy");
        }

        return PolicyGuardResult.Allowed();
    }
}
