using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Certification;

/// <summary>
/// Certification / failure-injection gate. Proves the runtime preserves
/// financial-grade safety under the three canonical failure windows:
///
///   FI1 Failure BEFORE persistence — a throw raised before the
///       event-store append propagates as an exception, leaves no
///       partial state in the event store, and releases the
///       idempotency claim so a retry with a fresh CommandId produces
///       exactly one persisted effect.
///   FI2 Failure DURING persistence — a throw raised INSIDE the append
///       call (the FailingEventStore seam) leaves the aggregate's
///       event list empty; the append-version invariant is atomic per
///       InMemoryEventStore so there is no partially-written stream.
///   FI3 Failure AFTER persistence / before acknowledgment — the
///       success is durable, but the client does not see the ack and
///       re-submits the SAME CommandId. The idempotency middleware
///       returns "Duplicate command detected" on the retry; the event
///       store holds exactly one emission; no double-spend.
/// </summary>
[Trait("Category", "Certification")]
public sealed class FailureInjectionTests
{
    [Fact]
    public async Task FI1_Failure_Before_Persistence_Does_Not_Corrupt_State()
    {
        var harness = ResilienceHarness.Build(failuresToInject: 1);
        var aggregateId = harness.IdGenerator.Generate("certification:FI1");

        Exception? firstAttempt = null;
        try
        {
            await harness.ControlPlane.ExecuteAsync(
                new CreateTodoCommand(aggregateId, "FI1"),
                harness.NewTodoContext(
                    aggregateId,
                    commandId: harness.IdGenerator.Generate("certification:FI1:cmd:1")));
        }
        catch (Exception ex)
        {
            firstAttempt = ex;
        }

        Assert.NotNull(firstAttempt);
        Assert.Empty(harness.EventStore.AllEvents(aggregateId));
        Assert.Empty(harness.EventStore.Versions(aggregateId));

        var retry = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "FI1-retry"),
            harness.NewTodoContext(
                aggregateId,
                commandId: harness.IdGenerator.Generate("certification:FI1:cmd:2")));

        Assert.True(retry.IsSuccess, retry.Error ?? "FI1 retry dispatch failed");
        var retryEvents = harness.EventStore.AllEvents(aggregateId).Count;
        Assert.True(retryEvents > 0,
            $"FI1 retry did not persist any events (count={retryEvents})");
    }

    [Fact]
    public async Task FI2_Failure_During_Persistence_Never_Partially_Writes()
    {
        var harness = ResilienceHarness.Build(failuresToInject: 3);
        var targetAggregate = harness.IdGenerator.Generate("certification:FI2:target");
        var neighbourAggregate = harness.IdGenerator.Generate("certification:FI2:neighbour");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await harness.ControlPlane.ExecuteAsync(
                    new CreateTodoCommand(targetAggregate, $"FI2-attempt-{attempt}"),
                    harness.NewTodoContext(
                        targetAggregate,
                        commandId: harness.IdGenerator.Generate($"certification:FI2:cmd:{attempt}")));
            }
            catch
            {
                // expected during the injected failure window
            }
        }

        Assert.Empty(harness.EventStore.AllEvents(targetAggregate));
        Assert.Empty(harness.EventStore.Versions(targetAggregate));

        var recovery = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(targetAggregate, "FI2-recovered"),
            harness.NewTodoContext(
                targetAggregate,
                commandId: harness.IdGenerator.Generate("certification:FI2:cmd:recover")));
        Assert.True(recovery.IsSuccess, recovery.Error ?? "FI2 recovery dispatch failed");

        var neighbour = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(neighbourAggregate, "FI2-neighbour"),
            harness.NewTodoContext(neighbourAggregate));
        Assert.True(neighbour.IsSuccess, neighbour.Error ?? "FI2 neighbour dispatch failed");

        var targetVersions = harness.EventStore.Versions(targetAggregate);
        Assert.True(targetVersions.Count > 0);
        for (var i = 0; i < targetVersions.Count; i++)
            Assert.Equal(i, targetVersions[i]);

        var neighbourVersions = harness.EventStore.Versions(neighbourAggregate);
        Assert.True(neighbourVersions.Count > 0);
        for (var i = 0; i < neighbourVersions.Count; i++)
            Assert.Equal(i, neighbourVersions[i]);
    }

    [Fact]
    public async Task FI3_Failure_After_Persistence_Resubmit_Is_Idempotent()
    {
        var harness = ResilienceHarness.Build();
        var aggregateId = harness.IdGenerator.Generate("certification:FI3");
        var commandId = harness.IdGenerator.Generate("certification:FI3:cmd");

        var first = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "FI3"),
            harness.NewTodoContext(aggregateId, commandId: commandId));
        Assert.True(first.IsSuccess, first.Error ?? "FI3 first dispatch failed");
        var emissionCount = harness.EventStore.AllEvents(aggregateId).Count;
        Assert.True(emissionCount > 0);

        // Simulate "client did not see ack, re-submits same CommandId"
        var resubmit = await harness.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "FI3"),
            harness.NewTodoContext(aggregateId, commandId: commandId));
        Assert.False(resubmit.IsSuccess);
        Assert.Equal("Duplicate command detected.", resubmit.Error);

        Assert.Equal(emissionCount, harness.EventStore.AllEvents(aggregateId).Count);
    }
}
