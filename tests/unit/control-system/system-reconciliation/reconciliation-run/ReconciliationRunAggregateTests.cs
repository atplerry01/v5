using Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemReconciliation.ReconciliationRun;

public sealed class ReconciliationRunAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset StartedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CompletedAt = new(2026, 4, 22, 10, 30, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ReconciliationRunId NewId(string seed) =>
        new(Hex64($"ReconciliationRunTests:{seed}:run"));

    [Fact]
    public void Schedule_RaisesReconciliationRunScheduledEvent()
    {
        var id = NewId("Schedule");

        var aggregate = ReconciliationRunAggregate.Schedule(id, "control-system");

        var evt = Assert.IsType<ReconciliationRunScheduledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("control-system", evt.Scope);
    }

    [Fact]
    public void Schedule_SetsStatusToPending()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("State"), "scope");

        Assert.Equal(RunStatus.Pending, aggregate.Status);
    }

    [Fact]
    public void Schedule_WithEmptyScope_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ReconciliationRunAggregate.Schedule(NewId("EmptyScope"), ""));
    }

    [Fact]
    public void Start_RaisesReconciliationRunStartedEvent()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("Start"), "scope");
        aggregate.ClearDomainEvents();

        aggregate.Start(StartedAt);

        Assert.IsType<ReconciliationRunStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RunStatus.Running, aggregate.Status);
    }

    [Fact]
    public void Start_WhenNotPending_Throws()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("StartNotPending"), "scope");
        aggregate.Start(StartedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Start(StartedAt.AddMinutes(1)));
    }

    [Fact]
    public void Complete_RaisesReconciliationRunCompletedEvent()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("Complete"), "scope");
        aggregate.Start(StartedAt);
        aggregate.ClearDomainEvents();

        aggregate.Complete(100, 3, CompletedAt);

        var evt = Assert.IsType<ReconciliationRunCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(100, evt.ChecksProcessed);
        Assert.Equal(3, evt.DiscrepanciesFound);
        Assert.Equal(RunStatus.Completed, aggregate.Status);
        Assert.Equal(100, aggregate.ChecksProcessed);
        Assert.Equal(3, aggregate.DiscrepanciesFound);
    }

    [Fact]
    public void Complete_WhenNotRunning_Throws()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("CompleteNotRunning"), "scope");

        Assert.ThrowsAny<Exception>(() => aggregate.Complete(0, 0, CompletedAt));
    }

    [Fact]
    public void Abort_RaisesReconciliationRunAbortedEvent()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("Abort"), "scope");
        aggregate.Start(StartedAt);
        aggregate.ClearDomainEvents();

        aggregate.Abort("disk full", CompletedAt);

        Assert.IsType<ReconciliationRunAbortedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RunStatus.Aborted, aggregate.Status);
    }

    [Fact]
    public void Abort_WithEmptyReason_Throws()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("AbortEmptyReason"), "scope");
        aggregate.Start(StartedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Abort("", CompletedAt));
    }

    [Fact]
    public void Abort_WhenNotRunning_Throws()
    {
        var aggregate = ReconciliationRunAggregate.Schedule(NewId("AbortNotRunning"), "scope");

        Assert.ThrowsAny<Exception>(() => aggregate.Abort("reason", CompletedAt));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new ReconciliationRunScheduledEvent(id, "control-system")
        };
        var aggregate = (ReconciliationRunAggregate)Activator.CreateInstance(typeof(ReconciliationRunAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(RunStatus.Pending, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
