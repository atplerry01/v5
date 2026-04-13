using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
/// pure-function decision over the runtime's maintenance posture
/// (<see cref="RuntimeMaintenanceMode"/>), the dispatch-time
/// degraded posture (<see cref="RuntimeDegradedMode"/>), and the
/// command's opt-in <see cref="IRestrictedDuringDegraded"/>
/// marker. Single source of truth for the enforcement rule —
/// consumed by <c>RuntimeControlPlane</c> on the dispatch hot
/// path and by the HC-8 unit tests directly.
///
/// Rule (in canonical evaluation order — first match wins):
///   1. Maintenance mode is active                            → BlockMaintenance ("system_maintenance_mode")
///   2. Degraded AND command implements IRestrictedDuringDegraded → BlockRestricted ("restricted_during_degraded_mode")
///   3. Degraded (any command)                                 → ProceedRestricted (tag context, do not block)
///   4. Otherwise                                              → Proceed
/// </summary>
public static class RuntimeEnforcementGate
{
    public const string ReasonMaintenance = "system_maintenance_mode";
    public const string ReasonRestricted = "restricted_during_degraded_mode";

    public static RuntimeEnforcementDecision Evaluate(
        RuntimeMaintenanceMode maintenance,
        RuntimeDegradedMode degraded,
        object command)
    {
        ArgumentNullException.ThrowIfNull(maintenance);
        ArgumentNullException.ThrowIfNull(degraded);
        ArgumentNullException.ThrowIfNull(command);

        if (maintenance.IsMaintenance)
            return new RuntimeEnforcementDecision(
                RuntimeEnforcementOutcome.BlockMaintenance,
                ReasonMaintenance);

        if (degraded.IsDegraded && command is IRestrictedDuringDegraded)
            return new RuntimeEnforcementDecision(
                RuntimeEnforcementOutcome.BlockRestricted,
                ReasonRestricted);

        if (degraded.IsDegraded)
            return new RuntimeEnforcementDecision(
                RuntimeEnforcementOutcome.ProceedRestricted,
                Reason: null);

        return new RuntimeEnforcementDecision(
            RuntimeEnforcementOutcome.Proceed,
            Reason: null);
    }
}

public enum RuntimeEnforcementOutcome
{
    Proceed = 0,
    ProceedRestricted = 1,
    BlockMaintenance = 2,
    BlockRestricted = 3,
}

public sealed record RuntimeEnforcementDecision(
    RuntimeEnforcementOutcome Outcome,
    string? Reason);
