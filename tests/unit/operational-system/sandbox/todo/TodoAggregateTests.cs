using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.OperationalSystem.Sandbox.Todo;

public sealed class TodoAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static TodoId NewId(string seed) =>
        new(IdGen.Generate($"TodoAggregateTests:{seed}:todo"));

    [Fact]
    public void Create_RaisesTodoCreatedEvent()
    {
        var id = NewId("Create_Valid");

        var aggregate = TodoAggregate.Create(id, "Write triage document");

        var evt = Assert.IsType<TodoCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id.Value, evt.AggregateId.Value);
        Assert.Equal("Write triage document", evt.Title);
    }

    [Fact]
    public void Create_SetsStateFromEvent()
    {
        var id = NewId("Create_State");

        var aggregate = TodoAggregate.Create(id, "State test");

        Assert.Equal(id, aggregate.Id);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var t1 = TodoAggregate.Create(id, "Stable title");
        var t2 = TodoAggregate.Create(id, "Stable title");

        Assert.Equal(
            ((TodoCreatedEvent)t1.DomainEvents[0]).AggregateId.Value,
            ((TodoCreatedEvent)t2.DomainEvents[0]).AggregateId.Value);
    }

    [Fact]
    public void Create_EmptyTitle_Throws()
    {
        var id = NewId("Empty_Title");

        Assert.ThrowsAny<Exception>(() => TodoAggregate.Create(id, "   "));
    }

    [Fact]
    public void ReviseTitle_RaisesTodoTitleRevisedEvent()
    {
        var id = NewId("Revise_Valid");
        var aggregate = TodoAggregate.Create(id, "Original title");
        aggregate.ClearDomainEvents();

        aggregate.ReviseTitle("Updated title");

        var evt = Assert.IsType<TodoTitleRevisedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("Updated title", evt.Title);
    }

    [Fact]
    public void Complete_RaisesTodoCompletedEvent()
    {
        var id = NewId("Complete_Valid");
        var aggregate = TodoAggregate.Create(id, "Finish this");
        aggregate.ClearDomainEvents();

        aggregate.Complete();

        Assert.IsType<TodoCompletedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Complete_Twice_Throws()
    {
        var id = NewId("Complete_Twice");
        var aggregate = TodoAggregate.Create(id, "Already done");
        aggregate.Complete();

        Assert.ThrowsAny<Exception>(() => aggregate.Complete());
    }

    [Fact]
    public void ReviseTitle_AfterComplete_Throws()
    {
        var id = NewId("Revise_AfterComplete");
        var aggregate = TodoAggregate.Create(id, "Done item");
        aggregate.Complete();

        Assert.ThrowsAny<Exception>(() => aggregate.ReviseTitle("Should not work"));
    }

    [Fact]
    public void LoadFromHistory_RehydratesTodoState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new TodoCreatedEvent(new AggregateId(id.Value), "History todo")
        };

        var aggregate = (TodoAggregate)Activator.CreateInstance(typeof(TodoAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Empty(aggregate.DomainEvents);
    }
}
