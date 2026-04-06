using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Systems.Midstream.Wss.Policy;

/// <summary>
/// Policy guard for WSS workflow steps.
/// READ-ONLY: reads the PolicyDecision already computed by PolicyMiddleware.
/// Does NOT invoke policy evaluation — that happens ONCE in the runtime pipeline.
/// </summary>
public static class WorkflowPolicyGuard
{
    public static PolicyGuardResult Enforce(IPolicyDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (!decision.IsAllowed)
        {
            return PolicyGuardResult.Denied(
                decision.Reason ?? "Workflow step denied by policy");
        }

        return PolicyGuardResult.Allowed();
    }
}
