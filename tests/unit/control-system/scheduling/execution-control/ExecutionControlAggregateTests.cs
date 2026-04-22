using Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Scheduling.ExecutionControl;

public sealed class ExecutionControlAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset IssuedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ExecutionControlId NewId(string seed) =>
        new(Hex64($"ExecutionControlTests:{seed}:ctrl"));

    [Fact]
    public void Issue_RaisesExecutionControlSignalIssuedEvent()
    {
        var id = NewId("Issue");

        var aggregate = ExecutionControlAggregate.Issue(id, "job-instance-1", ControlSignal.Start, "actor-1", IssuedAt);

        var evt = Assert.IsType<ExecutionControlSignalIssuedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("job-instance-1", evt.JobInstanceId);
        Assert.Equal(ControlSignal.Start, evt.Signal);
        Assert.Equal("actor-1", evt.ActorId);
    }

    [Fact]
    public void Issue_SetsOutcomeToNull()
    {
        var aggregate = ExecutionControlAggregate.Issue(NewId("State"), "job-1", ControlSignal.Stop, "actor-1", IssuedAt);

        Assert.Null(aggregate.Outcome);
    }

    [Fact]
    public void Issue_WithEmptyJobInstanceId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ExecutionControlAggregate.Issue(NewId("EmptyJob"), "", ControlSignal.Start, "actor-1", IssuedAt));
    }

    [Fact]
    public void Issue_WithEmptyActorId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ExecutionControlAggregate.Issue(NewId("EmptyActor"), "job-1", ControlSignal.Start, "", IssuedAt));
    }

    [Fact]
    public void Apply_RaisesExecutionControlSignalOutcomeRecordedEvent()
    {
        var aggregate = ExecutionControlAggregate.Issue(NewId("Apply"), "job-1", ControlSignal.Suspend, "actor-1", IssuedAt);
        aggregate.ClearDomainEvents();

        aggregate.Apply(ControlSignalOutcome.Acknowledged);

        var evt = Assert.IsType<ExecutionControlSignalOutcomeRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ControlSignalOutcome.Acknowledged, evt.Outcome);
        Assert.Equal(ControlSignalOutcome.Acknowledged, aggregate.Outcome);
    }

    [Fact]
    public void Apply_AlreadyRecorded_Throws()
    {
        var aggregate = ExecutionControlAggregate.Issue(NewId("DoubleApply"), "job-1", ControlSignal.Resume, "actor-1", IssuedAt);
        aggregate.Apply(ControlSignalOutcome.Applied);

        Assert.ThrowsAny<Exception>(() => aggregate.Apply(ControlSignalOutcome.Rejected));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new ExecutionControlSignalIssuedEvent(id, "job-instance-1", ControlSignal.Start, "actor-1", IssuedAt)
        };
        var aggregate = (ExecutionControlAggregate)Activator.CreateInstance(typeof(ExecutionControlAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ControlSignal.Start, aggregate.Signal);
        Assert.Null(aggregate.Outcome);
        Assert.Empty(aggregate.DomainEvents);
    }
}
