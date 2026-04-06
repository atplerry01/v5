using Whycespace.Domain.EconomicSystem.Reconciliation.Reconciliation;

namespace Whycespace.Runtime.Economic.Result;

public sealed record ReconciliationExecutionResult
{
    public required bool IsHealthy { get; init; }
    public IReadOnlyCollection<RecoveryInstruction>? RecoveryActions { get; init; }

    public static ReconciliationExecutionResult Healthy() =>
        new() { IsHealthy = true };

    public static ReconciliationExecutionResult Unhealthy(IEnumerable<RecoveryInstruction> actions) =>
        new() { IsHealthy = false, RecoveryActions = actions.ToList() };
}
