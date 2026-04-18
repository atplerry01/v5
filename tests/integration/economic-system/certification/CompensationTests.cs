using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Certification;

/// <summary>
/// Certification / compensation gate. Proves the runtime supports the
/// two-command compensation pattern that drives saga-style recovery in
/// the economic-system:
///
///   CP1 Mid-flight failure leaves NO partial effect — when a forward
///       command is interrupted by a persistence failure, the event
///       store carries zero entries for that aggregate and any
///       subsequent compensating command can be issued against a
///       consistent baseline.
///   CP2 Compensation produces a fully audited trail — forward command
///       and compensating command both appear in the event stream for
///       the aggregate, preserving version monotonicity and the
///       before/after observability required for financial audit.
///   CP3 Compensation cannot double-apply — replaying the compensation
///       with the same CommandId is rejected by the idempotency
///       middleware; the aggregate state remains whatever the first
///       compensation produced.
/// </summary>
[Trait("Category", "Certification")]
public sealed class CompensationTests
{
    [Fact]
    public async Task CP1_Mid_Flight_Failure_Leaves_Consistent_Baseline()
    {
        var harness = ResilienceHarness.Build(failuresToInject: 1);
        var aggregateId = harness.IdGenerator.Generate("certification:CP1");

        Exception? midFlight = null;
        try
        {
            await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, "CP1-forward"),
                harness.NewTodoContext(
                    aggregateId,
                    commandId: harness.IdGenerator.Generate("certification:CP1:cmd:forward")));
        }
        catch (Exception ex)
        {
            midFlight = ex;
        }

        Assert.NotNull(midFlight);
        Assert.Empty(harness.EventStore.AllEvents(aggregateId));

        // After the mid-flight failure the aggregate has no state. A
        // compensating dispatch (fresh CommandId) must be able to move
        // the aggregate into a known, audited terminal state.
        var compensation = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "CP1-compensated"),
            harness.NewTodoContext(
                aggregateId,
                commandId: harness.IdGenerator.Generate("certification:CP1:cmd:compensate")));
        Assert.True(compensation.IsSuccess, compensation.Error ?? "CP1 compensation failed");

        var versions = harness.EventStore.Versions(aggregateId);
        Assert.True(versions.Count > 0);
        for (var i = 0; i < versions.Count; i++)
            Assert.Equal(i, versions[i]);
    }

    [Fact]
    public async Task CP2_Compensation_Produces_Auditable_Trail()
    {
        var harness = ResilienceHarness.Build();
        var aggregateId = harness.IdGenerator.Generate("certification:CP2");

        var forward = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "CP2-forward"),
            harness.NewTodoContext(
                aggregateId,
                commandId: harness.IdGenerator.Generate("certification:CP2:cmd:forward")));
        Assert.True(forward.IsSuccess, forward.Error ?? "CP2 forward failed");
        var forwardEvents = harness.EventStore.AllEvents(aggregateId).Count;
        var forwardVersions = harness.EventStore.Versions(aggregateId).Count;

        // Compensating action is modelled as CompleteTodo — the semantic
        // "cancel/close" terminal state in the Todo aggregate. The
        // production saga-style compensation always appends an auditable
        // event so the before/after state is reconstructable from the
        // event stream alone.
        var compensation = await harness.ControlPlane.ExecuteAsync(
            new CompleteTodoCommand(aggregateId),
            harness.NewTodoContext(
                aggregateId,
                commandId: harness.IdGenerator.Generate("certification:CP2:cmd:compensate")));
        Assert.True(compensation.IsSuccess, compensation.Error ?? "CP2 compensation failed");

        var totalEvents = harness.EventStore.AllEvents(aggregateId).Count;
        var totalVersions = harness.EventStore.Versions(aggregateId);

        Assert.True(totalEvents > forwardEvents,
            $"CP2 compensation did not append audit events: forwardEvents={forwardEvents} total={totalEvents}");
        Assert.True(totalVersions.Count > forwardVersions,
            $"CP2 compensation did not advance version stream: forwardVersions={forwardVersions} total={totalVersions.Count}");
        for (var i = 0; i < totalVersions.Count; i++)
            Assert.Equal(i, totalVersions[i]);
    }

    [Fact]
    public async Task CP3_Compensation_Replay_Does_Not_Double_Apply()
    {
        var harness = ResilienceHarness.Build();
        var aggregateId = harness.IdGenerator.Generate("certification:CP3");
        var compensateCmdId = harness.IdGenerator.Generate("certification:CP3:cmd:compensate");

        var forward = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "CP3-forward"),
            harness.NewTodoContext(
                aggregateId,
                commandId: harness.IdGenerator.Generate("certification:CP3:cmd:forward")));
        Assert.True(forward.IsSuccess, forward.Error ?? "CP3 forward failed");

        var compensation = await harness.ControlPlane.ExecuteAsync(
            new CompleteTodoCommand(aggregateId),
            harness.NewTodoContext(aggregateId, commandId: compensateCmdId));
        Assert.True(compensation.IsSuccess, compensation.Error ?? "CP3 compensation failed");

        var eventsAfterCompensation = harness.EventStore.AllEvents(aggregateId).Count;

        var replay = await harness.ControlPlane.ExecuteAsync(
            new CompleteTodoCommand(aggregateId),
            harness.NewTodoContext(aggregateId, commandId: compensateCmdId));
        Assert.False(replay.IsSuccess);
        Assert.Equal("Duplicate command detected.", replay.Error);

        Assert.Equal(eventsAfterCompensation, harness.EventStore.AllEvents(aggregateId).Count);
    }
}
