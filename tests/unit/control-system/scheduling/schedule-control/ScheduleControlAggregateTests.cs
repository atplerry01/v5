using Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Scheduling.ScheduleControl;

public sealed class ScheduleControlAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ScheduleControlId NewId(string seed) =>
        new(Hex64($"ScheduleControlTests:{seed}:sched"));

    [Fact]
    public void Define_RaisesScheduleControlDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = ScheduleControlAggregate.Define(id, "job-def-1", "0 * * * *");

        var evt = Assert.IsType<ScheduleControlDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("job-def-1", evt.JobDefinitionId);
        Assert.Equal("0 * * * *", evt.TriggerExpression);
    }

    [Fact]
    public void Define_SetsStatusToActive()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("State"), "job-def-1", "0 * * * *");

        Assert.Equal(ScheduleStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Define_WithEmptyJobDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ScheduleControlAggregate.Define(NewId("EmptyJob"), "", "0 * * * *"));
    }

    [Fact]
    public void Define_WithEmptyTriggerExpression_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ScheduleControlAggregate.Define(NewId("EmptyTrigger"), "job-def-1", ""));
    }

    [Fact]
    public void Suspend_RaisesScheduleControlSuspendedEvent()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("Suspend"), "job-def-1", "0 * * * *");
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<ScheduleControlSuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ScheduleStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Suspend_OnSuspendedSchedule_Throws()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("SuspendSuspended"), "job-def-1", "0 * * * *");
        aggregate.Suspend();

        Assert.ThrowsAny<Exception>(() => aggregate.Suspend());
    }

    [Fact]
    public void Resume_RaisesScheduleControlResumedEvent()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("Resume"), "job-def-1", "0 * * * *");
        aggregate.Suspend();
        aggregate.ClearDomainEvents();

        aggregate.Resume();

        Assert.IsType<ScheduleControlResumedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ScheduleStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Resume_OnActiveSchedule_Throws()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("ResumeActive"), "job-def-1", "0 * * * *");

        Assert.ThrowsAny<Exception>(() => aggregate.Resume());
    }

    [Fact]
    public void Retire_RaisesScheduleControlRetiredEvent()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("Retire"), "job-def-1", "0 * * * *");
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<ScheduleControlRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ScheduleStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Retire_AlreadyRetired_Throws()
    {
        var aggregate = ScheduleControlAggregate.Define(NewId("DoubleRetire"), "job-def-1", "0 * * * *");
        aggregate.Retire();

        Assert.ThrowsAny<Exception>(() => aggregate.Retire());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new ScheduleControlDefinedEvent(id, "job-def-1", "0 * * * *")
        };
        var aggregate = (ScheduleControlAggregate)Activator.CreateInstance(typeof(ScheduleControlAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ScheduleStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
