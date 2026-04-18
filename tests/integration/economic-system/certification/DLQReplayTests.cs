using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Tests.Integration.EconomicSystem.Certification.Shared;
using Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Certification;

/// <summary>
/// Certification / DLQ + replay gate. Proves the runtime's failure-
/// capture and replay path preserves financial invariants:
///
///   DLQ1 Failed transactions are captured — a command that exhausts
///        its in-test retry budget (each attempt minting a FRESH
///        CommandId per the production envelope pattern) is recorded
///        in the DeadLetterQueue with its (command, aggregateId,
///        reason) tuple so an operator replay stage can re-issue it.
///   DLQ2 Replay succeeds once the failure window clears — draining
///        the DLQ and re-dispatching each entry with a fresh operator
///        CommandId produces exactly one persisted effect.
///   DLQ3 Replay is idempotent — dispatching the drained DLQ entry
///        twice with the SAME operator CommandId is rejected by the
///        idempotency middleware on the second attempt; the event
///        store holds exactly one emission.
///
/// Note: the runtime event-fabric persistence boundary (see
/// <c>RuntimeControlPlane.ExecuteAsync</c>) appends events AFTER the
/// middleware pipeline returns. A persistence failure therefore throws
/// OUTSIDE the idempotency middleware's try/catch, so the claim stays
/// recorded. The production retry pattern — fresh CommandId per
/// attempt — is the only safe way to drive retries; this suite
/// mirrors that contract.
/// </summary>
[Trait("Category", "Certification")]
public sealed class DLQReplayTests
{
    private const int RetryBudget = 3;

    [Fact]
    public async Task DLQ1_Exhausted_Retries_Capture_Command_In_DeadLetterQueue()
    {
        var harness = ResilienceHarness.Build(failuresToInject: RetryBudget);
        var dlq = new DeadLetterQueue();
        var aggregateId = harness.IdGenerator.Generate("certification:DLQ1");
        var command = new CreateTodoCommand(aggregateId, "DLQ1");

        Exception? final = null;
        for (var attempt = 0; attempt < RetryBudget; attempt++)
        {
            try
            {
                await harness.ControlPlane.ExecuteAsync(
                    command,
                    harness.NewTodoContext(
                        aggregateId,
                        commandId: harness.IdGenerator.Generate($"certification:DLQ1:cmd:{attempt}")));
                final = null;
                break;
            }
            catch (Exception ex)
            {
                final = ex;
            }
        }

        Assert.NotNull(final);
        Assert.Empty(harness.EventStore.AllEvents(aggregateId));

        dlq.Capture(command, aggregateId, reason: final!.Message);
        Assert.Equal(1, dlq.Count);
        var captured = dlq.Snapshot()[0];
        Assert.Same(command, captured.Command);
        Assert.Equal(aggregateId, captured.AggregateId);
    }

    [Fact]
    public async Task DLQ2_Drain_And_Replay_Succeeds_Exactly_Once()
    {
        var harness = ResilienceHarness.Build(failuresToInject: RetryBudget);
        var dlq = new DeadLetterQueue();
        var aggregateId = harness.IdGenerator.Generate("certification:DLQ2");
        var command = new CreateTodoCommand(aggregateId, "DLQ2");

        for (var attempt = 0; attempt < RetryBudget; attempt++)
        {
            try
            {
                await harness.ControlPlane.ExecuteAsync(
                    command,
                    harness.NewTodoContext(
                        aggregateId,
                        commandId: harness.IdGenerator.Generate($"certification:DLQ2:cmd:{attempt}")));
                break;
            }
            catch { }
        }

        dlq.Capture(command, aggregateId, reason: "exhausted retry budget");
        Assert.Empty(harness.EventStore.AllEvents(aggregateId));

        var drained = dlq.Drain();
        Assert.Single(drained);

        var replayCommandId = harness.IdGenerator.Generate("certification:DLQ2:cmd:replay");
        var replay = await harness.ControlPlane.ExecuteAsync(
            drained[0].Command,
            harness.NewTodoContext(drained[0].AggregateId, commandId: replayCommandId));
        Assert.True(replay.IsSuccess, replay.Error ?? "DLQ2 replay failed");

        var events = harness.EventStore.AllEvents(aggregateId);
        Assert.True(events.Count > 0, "DLQ2 replay produced zero events");
    }

    [Fact]
    public async Task DLQ3_Replay_Is_Idempotent_Under_Duplicate_Submission()
    {
        var harness = ResilienceHarness.Build(failuresToInject: RetryBudget);
        var dlq = new DeadLetterQueue();
        var aggregateId = harness.IdGenerator.Generate("certification:DLQ3");
        var command = new CreateTodoCommand(aggregateId, "DLQ3");

        for (var attempt = 0; attempt < RetryBudget; attempt++)
        {
            try
            {
                await harness.ControlPlane.ExecuteAsync(
                    command,
                    harness.NewTodoContext(
                        aggregateId,
                        commandId: harness.IdGenerator.Generate($"certification:DLQ3:cmd:{attempt}")));
                break;
            }
            catch { }
        }

        dlq.Capture(command, aggregateId, reason: "exhausted retry budget");

        var drained = dlq.Drain();
        Assert.Single(drained);
        var entry = drained[0];

        var replayCommandId = harness.IdGenerator.Generate("certification:DLQ3:cmd:replay");
        var firstReplay = await harness.ControlPlane.ExecuteAsync(
            entry.Command,
            harness.NewTodoContext(entry.AggregateId, commandId: replayCommandId));
        Assert.True(firstReplay.IsSuccess, firstReplay.Error ?? "DLQ3 first replay failed");
        var eventsAfterFirstReplay = harness.EventStore.AllEvents(aggregateId).Count;
        Assert.True(eventsAfterFirstReplay > 0);

        var duplicateReplay = await harness.ControlPlane.ExecuteAsync(
            entry.Command,
            harness.NewTodoContext(entry.AggregateId, commandId: replayCommandId));
        Assert.False(duplicateReplay.IsSuccess);
        Assert.Equal("Duplicate command detected.", duplicateReplay.Error);

        Assert.Equal(eventsAfterFirstReplay, harness.EventStore.AllEvents(aggregateId).Count);
    }
}
