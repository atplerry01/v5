using Whycespace.Domain.Operational.Sandbox.Todo;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.Operational.Sandbox.Todo;

public sealed class TodoAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void Create_WithValidTitle_RaisesTodoCreatedEvent()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:Create_Valid:id"));

        var aggregate = TodoAggregate.Create(id, "Buy groceries");

        var events = aggregate.DomainEvents;
        Assert.Single(events);
        var created = Assert.IsType<TodoCreatedEvent>(events[0]);
        Assert.Equal(id.Value, created.AggregateId.Value);
        Assert.Equal("Buy groceries", created.Title);
    }

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:Create_Empty:id"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            TodoAggregate.Create(id, ""));
    }

    [Fact]
    public void Update_WithValidTitle_RaisesTodoUpdatedEvent()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:Update_Valid:id"));
        var aggregate = TodoAggregate.Create(id, "Original");
        aggregate.ClearDomainEvents();

        aggregate.Update("Updated title");

        var events = aggregate.DomainEvents;
        Assert.Single(events);
        var updated = Assert.IsType<TodoUpdatedEvent>(events[0]);
        Assert.Equal("Updated title", updated.Title);
    }

    [Fact]
    public void Update_WhenCompleted_Throws()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:Update_Completed:id"));
        var aggregate = TodoAggregate.Create(id, "Task");
        aggregate.Complete();

        Assert.Throws<DomainInvariantViolationException>(() =>
            aggregate.Update("New title"));
    }

    [Fact]
    public void Complete_RaisesTodoCompletedEvent()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:Complete:id"));
        var aggregate = TodoAggregate.Create(id, "Task");
        aggregate.ClearDomainEvents();

        aggregate.Complete();

        var events = aggregate.DomainEvents;
        Assert.Single(events);
        Assert.IsType<TodoCompletedEvent>(events[0]);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Throws()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:Complete_Already:id"));
        var aggregate = TodoAggregate.Create(id, "Task");
        aggregate.Complete();

        Assert.Throws<DomainInvariantViolationException>(() =>
            aggregate.Complete());
    }

    [Fact]
    public void LoadFromHistory_ReconstitutesState()
    {
        var aggregateId = new AggregateId(IdGen.Generate("TodoAggregateTests:LoadFromHistory:id"));
        var events = new object[]
        {
            new TodoCreatedEvent(aggregateId, "Test"),
            new TodoUpdatedEvent(aggregateId, "Updated"),
            new TodoCompletedEvent(aggregateId)
        };

        // Reconstitute via LoadFromHistory
        var aggregate = (TodoAggregate)Activator.CreateInstance(typeof(TodoAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(events);

        Assert.Equal(2, aggregate.Version); // 0-indexed: 3 events = version 2
        Assert.Empty(aggregate.DomainEvents); // No uncommitted events
    }

    [Fact]
    public void FullLifecycle_Create_Update_Complete()
    {
        var id = new TodoId(IdGen.Generate("TodoAggregateTests:FullLifecycle:id"));

        var aggregate = TodoAggregate.Create(id, "Initial");
        aggregate.Update("Modified");
        aggregate.Complete();

        Assert.Equal(3, aggregate.DomainEvents.Count);
        Assert.IsType<TodoCreatedEvent>(aggregate.DomainEvents[0]);
        Assert.IsType<TodoUpdatedEvent>(aggregate.DomainEvents[1]);
        Assert.IsType<TodoCompletedEvent>(aggregate.DomainEvents[2]);
    }
}
