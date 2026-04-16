using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Domain.ConstitutionalSystem.Policy.Decision;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Operational.Sandbox.Todo;

/// <summary>
/// Smoke test for the full Todo lifecycle through the real RuntimeControlPlane.
/// Drives the locked 8-middleware pipeline + 3-stage event fabric end-to-end.
/// </summary>
public sealed class TodoPipelineTests
{
    [Fact]
    public async Task FullLifecycle_Create_Update_Complete()
    {
        var host = TestHost.ForTodo();
        var aggregateId = host.IdGenerator.Generate("todo:lifecycle:1");

        var createResult = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Buy milk"),
            host.NewTodoContext(aggregateId));
        Assert.True(createResult.IsSuccess, createResult.Error);

        var updateResult = await host.ControlPlane.ExecuteAsync(
            new UpdateTodoCommand(aggregateId, "Buy almond milk"),
            host.NewTodoContext(aggregateId));
        Assert.True(updateResult.IsSuccess, updateResult.Error);

        var completeResult = await host.ControlPlane.ExecuteAsync(
            new CompleteTodoCommand(aggregateId),
            host.NewTodoContext(aggregateId));
        Assert.True(completeResult.IsSuccess, completeResult.Error);

        // Three domain events persisted under the Todo aggregate stream
        // (PolicyEvaluatedEvent now flows to a dedicated policy decision stream
        // via AuditEmission — see PolicyEventificationTests).
        var stored = host.EventStore.AllEvents(aggregateId);
        Assert.Equal(3, stored.Count);
        Assert.IsType<TodoCreatedEvent>(stored[0]);
        Assert.IsType<TodoTitleRevisedEvent>(stored[1]);
        Assert.IsType<TodoCompletedEvent>(stored[2]);
        Assert.DoesNotContain(stored, e => e is PolicyEvaluatedEvent);

        // Six chain blocks + outbox batches: each command emits one audit batch
        // (policy decision) THEN one domain batch (todo event).
        Assert.Equal(6, host.ChainAnchor.Blocks.Count);
        Assert.Equal(6, host.Outbox.Batches.Count);
        // Audit batches publish to the dedicated constitutional policy topic.
        Assert.Equal("whyce.constitutional.policy.decision.events", host.Outbox.Batches[0].Topic);
        Assert.Equal("whyce.operational.sandbox.todo.events", host.Outbox.Batches[1].Topic);

    }

    [Fact]
    public async Task PolicyDenial_BlocksExecution_But_Records_DeniedEvent()
    {
        var host = TestHost.ForTodo(denyPolicy: true);
        var aggregateId = host.IdGenerator.Generate("todo:denied:1");

        var result = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Blocked"),
            host.NewTodoContext(aggregateId));

        Assert.False(result.IsSuccess);

        // The Todo aggregate stream stays empty — denial blocked the command
        // before the engine ran. The PolicyDeniedEvent flows to its own
        // dedicated audit stream (covered by PolicyEventificationTests).
        Assert.Empty(host.EventStore.AllEvents(aggregateId));

        // Audit emission still produces one chain block + one outbox batch on
        // the constitutional policy topic.
        Assert.Single(host.ChainAnchor.Blocks);
        var batch = Assert.Single(host.Outbox.Batches);
        Assert.Equal("whyce.constitutional.policy.decision.events", batch.Topic);
    }
}
