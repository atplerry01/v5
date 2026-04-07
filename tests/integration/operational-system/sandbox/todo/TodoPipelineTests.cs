using Whyce.Shared.Contracts.Application.Todo;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.OperationalSystem.Sandbox.Todo;

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

        // Three events persisted, three chain blocks, three outbox batches.
        var stored = host.EventStore.AllEvents(aggregateId);
        Assert.Equal(3, stored.Count);
        Assert.IsType<TodoCreatedEvent>(stored[0]);
        Assert.IsType<TodoUpdatedEvent>(stored[1]);
        Assert.IsType<TodoCompletedEvent>(stored[2]);

        Assert.Equal(3, host.ChainAnchor.Blocks.Count);
        Assert.Equal(3, host.Outbox.Batches.Count);

        // Outbox topic resolution exercises TopicNameResolver against the real envelope.
        Assert.Equal("whyce.operational.sandbox.todo.events", host.Outbox.Batches[0].Topic);
    }

    [Fact]
    public async Task PolicyDenial_BlocksExecution()
    {
        var host = TestHost.ForTodo(denyPolicy: true);
        var aggregateId = host.IdGenerator.Generate("todo:denied:1");

        var result = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Blocked"),
            host.NewTodoContext(aggregateId));

        Assert.False(result.IsSuccess);
        Assert.Empty(host.EventStore.AllEvents(aggregateId));
        Assert.Empty(host.ChainAnchor.Blocks);
        Assert.Empty(host.Outbox.Batches);
    }
}
