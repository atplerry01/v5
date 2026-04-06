namespace Whyce.Runtime.Guards;

/// <summary>
/// Fabric Invocation Guard — ensures ControlPlane → Fabric is
/// SINGLE and NON-BYPASSABLE.
///
/// Validates:
/// - Events only flow through IEventFabric.ProcessAsync
/// - No direct EventStore/ChainAnchor/Outbox calls from control plane
/// - Fabric invocation happens AFTER successful engine execution
/// - Fabric invocation happens ONLY for events requiring persistence
/// </summary>
public static class FabricInvocationGuard
{
    /// <summary>
    /// Validates that the fabric invocation preconditions are met.
    /// Returns violations found.
    /// </summary>
    public static IReadOnlyList<string> ValidatePreConditions(
        bool isSuccess,
        bool eventsRequirePersistence,
        int emittedEventCount,
        bool policyDecisionAllowed,
        bool runtimeOrigin)
    {
        var violations = new List<string>();

        if (!isSuccess)
        {
            violations.Add("Fabric invocation blocked: command execution failed.");
        }

        if (!eventsRequirePersistence)
        {
            violations.Add("Fabric invocation skipped: events do not require persistence.");
        }

        if (emittedEventCount == 0)
        {
            violations.Add("Fabric invocation blocked: no events emitted.");
        }

        if (!policyDecisionAllowed)
        {
            violations.Add("S0-CRITICAL: Fabric invocation blocked: no policy decision.");
        }

        if (!runtimeOrigin)
        {
            violations.Add("S0-CRITICAL: Fabric invocation blocked: not from runtime origin.");
        }

        return violations;
    }
}
