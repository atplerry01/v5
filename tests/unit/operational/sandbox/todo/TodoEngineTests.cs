using Whycespace.Engines.T2E.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.Operational.Sandbox.Todo;

public sealed class TodoEngineTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public async Task ExecuteAsync_CreateCommand_EmitsCreatedEvent()
    {
        var handler = new CreateTodoHandler();
        var aggregateId = IdGen.Generate("TodoEngineTests:Create:agg");
        var command = new CreateTodoCommand(aggregateId, "Test todo");

        var context = new EngineContext(
            command,
            aggregateId,
            (type, id) => Task.FromResult<object>(null!));

        await handler.ExecuteAsync(context);

        Assert.Single(context.EmittedEvents);
        var created = Assert.IsType<TodoCreatedEvent>(context.EmittedEvents[0]);
        Assert.Equal("Test todo", created.Title);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateCommand_EmitsUpdatedEvent()
    {
        var handler = new UpdateTodoHandler();
        var aggregateId = IdGen.Generate("TodoEngineTests:Update:agg");
        var command = new UpdateTodoCommand(aggregateId, "Updated");

        // Pre-create aggregate with history
        var aggregate = TodoAggregate.Create(new TodoId(aggregateId), "Original");
        aggregate.ClearDomainEvents();

        var context = new EngineContext(
            command,
            aggregateId,
            (type, id) => Task.FromResult<object>(aggregate));

        await handler.ExecuteAsync(context);

        Assert.Single(context.EmittedEvents);
        var updated = Assert.IsType<TodoUpdatedEvent>(context.EmittedEvents[0]);
        Assert.Equal("Updated", updated.Title);
    }

    [Fact]
    public async Task ExecuteAsync_CompleteCommand_EmitsCompletedEvent()
    {
        var handler = new CompleteTodoHandler();
        var aggregateId = IdGen.Generate("TodoEngineTests:Complete:agg");
        var command = new CompleteTodoCommand(aggregateId);

        var aggregate = TodoAggregate.Create(new TodoId(aggregateId), "Task");
        aggregate.ClearDomainEvents();

        var context = new EngineContext(
            command,
            aggregateId,
            (type, id) => Task.FromResult<object>(aggregate));

        await handler.ExecuteAsync(context);

        Assert.Single(context.EmittedEvents);
        Assert.IsType<TodoCompletedEvent>(context.EmittedEvents[0]);
    }
}
