using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Runtime;
using Xunit;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
/// unit coverage for the pure RuntimeEnforcementGate.Evaluate
/// rule. The gate is the single source of truth for the runtime
/// control plane's HC-8 hard-block / soft-tag decision; testing it
/// in isolation lets us validate the rule without spinning up the
/// control plane, middleware pipeline, or DI container.
/// </summary>
public sealed class RuntimeEnforcementGateTests
{
    private sealed record NormalCommand;
    private sealed record RestrictedCommand : IRestrictedDuringDegraded;

    [Fact]
    public void Maintenance_HardBlocks_AnyCommand()
    {
        var maintenance = new RuntimeMaintenanceMode(true, new[] { "operator_declared" });
        var degraded = RuntimeDegradedMode.None;
        var decision = RuntimeEnforcementGate.Evaluate(maintenance, degraded, new NormalCommand());

        Assert.Equal(RuntimeEnforcementOutcome.BlockMaintenance, decision.Outcome);
        Assert.Equal(RuntimeEnforcementGate.ReasonMaintenance, decision.Reason);
    }

    [Fact]
    public void Maintenance_DominatesDegraded_RestrictedCommand()
    {
        // Maintenance is the highest-priority rule — even a
        // restricted-during-degraded command sees the maintenance
        // reason, not the restricted reason.
        var maintenance = new RuntimeMaintenanceMode(true, new[] { "schema_migration" });
        var degraded = RuntimeDegradedMode.From(new[] { "opa_breaker_open" });
        var decision = RuntimeEnforcementGate.Evaluate(maintenance, degraded, new RestrictedCommand());

        Assert.Equal(RuntimeEnforcementOutcome.BlockMaintenance, decision.Outcome);
        Assert.Equal(RuntimeEnforcementGate.ReasonMaintenance, decision.Reason);
    }

    [Fact]
    public void Degraded_NormalCommand_ProceedsRestricted_NoBlock()
    {
        var maintenance = RuntimeMaintenanceMode.None;
        var degraded = RuntimeDegradedMode.From(new[] { "opa_breaker_open" });
        var decision = RuntimeEnforcementGate.Evaluate(maintenance, degraded, new NormalCommand());

        Assert.Equal(RuntimeEnforcementOutcome.ProceedRestricted, decision.Outcome);
        Assert.Null(decision.Reason);
    }

    [Fact]
    public void Degraded_RestrictedCommand_HardBlocks()
    {
        var maintenance = RuntimeMaintenanceMode.None;
        var degraded = RuntimeDegradedMode.From(new[] { "chain_anchor_breaker_open" });
        var decision = RuntimeEnforcementGate.Evaluate(maintenance, degraded, new RestrictedCommand());

        Assert.Equal(RuntimeEnforcementOutcome.BlockRestricted, decision.Outcome);
        Assert.Equal(RuntimeEnforcementGate.ReasonRestricted, decision.Reason);
    }

    [Fact]
    public void Normal_ProceedsCleanly()
    {
        var decision = RuntimeEnforcementGate.Evaluate(
            RuntimeMaintenanceMode.None,
            RuntimeDegradedMode.None,
            new NormalCommand());

        Assert.Equal(RuntimeEnforcementOutcome.Proceed, decision.Outcome);
        Assert.Null(decision.Reason);
    }

    [Fact]
    public void Normal_RestrictedCommand_NotInDegraded_ProceedsCleanly()
    {
        // The restricted marker only matters when degraded is
        // active; under a healthy posture the marker is inert.
        var decision = RuntimeEnforcementGate.Evaluate(
            RuntimeMaintenanceMode.None,
            RuntimeDegradedMode.None,
            new RestrictedCommand());

        Assert.Equal(RuntimeEnforcementOutcome.Proceed, decision.Outcome);
        Assert.Null(decision.Reason);
    }
}
