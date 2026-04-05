using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.OperationalSystem.Sandbox.Todo;

public sealed class TodoEngineTests
{
    [Fact]
    public async Task ExecuteAsync_CreateCommand_EmitsCreatedEvent()
    {
        var engine = new TodoEngine();
        var aggregateId = Guid.NewGuid();
        var command = new CreateTodoCommand(aggregateId, "Test todo");

        var context = new EngineContext(
            command,
            aggregateId,
            (type, id) => Task.FromResult<object>(null!));

        await engine.ExecuteAsync(context);

        Assert.Single(context.EmittedEvents);
        var created = Assert.IsType<TodoCreatedEvent>(context.EmittedEvents[0]);
        Assert.Equal("Test todo", created.Title);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateCommand_EmitsUpdatedEvent()
    {
        var engine = new TodoEngine();
        var aggregateId = Guid.NewGuid();
        var command = new UpdateTodoCommand(aggregateId, "Updated");

        // Pre-create aggregate with history
        var aggregate = TodoAggregate.Create(new TodoId(aggregateId), "Original");
        aggregate.ClearDomainEvents();

        var context = new EngineContext(
            command,
            aggregateId,
            (type, id) => Task.FromResult<object>(aggregate));

        await engine.ExecuteAsync(context);

        Assert.Single(context.EmittedEvents);
        var updated = Assert.IsType<TodoUpdatedEvent>(context.EmittedEvents[0]);
        Assert.Equal("Updated", updated.Title);
    }

    [Fact]
    public async Task ExecuteAsync_CompleteCommand_EmitsCompletedEvent()
    {
        var engine = new TodoEngine();
        var aggregateId = Guid.NewGuid();
        var command = new CompleteTodoCommand(aggregateId);

        var aggregate = TodoAggregate.Create(new TodoId(aggregateId), "Task");
        aggregate.ClearDomainEvents();

        var context = new EngineContext(
            command,
            aggregateId,
            (type, id) => Task.FromResult<object>(aggregate));

        await engine.ExecuteAsync(context);

        Assert.Single(context.EmittedEvents);
        Assert.IsType<TodoCompletedEvent>(context.EmittedEvents[0]);
    }
}
