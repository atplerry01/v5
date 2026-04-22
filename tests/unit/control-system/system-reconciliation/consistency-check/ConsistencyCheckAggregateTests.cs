using Whycespace.Domain.ControlSystem.SystemReconciliation.ConsistencyCheck;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemReconciliation.ConsistencyCheck;

public sealed class ConsistencyCheckAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset InitiatedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CompletedAt = new(2026, 4, 22, 10, 5, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static ConsistencyCheckId NewId(string seed) =>
        new(Hex64($"ConsistencyCheckTests:{seed}:check"));

    [Fact]
    public void Initiate_RaisesConsistencyCheckInitiatedEvent()
    {
        var id = NewId("Initiate");

        var aggregate = ConsistencyCheckAggregate.Initiate(id, "control-system/audit", InitiatedAt);

        var evt = Assert.IsType<ConsistencyCheckInitiatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("control-system/audit", evt.ScopeTarget);
        Assert.Equal(InitiatedAt, evt.InitiatedAt);
    }

    [Fact]
    public void Initiate_SetsStatusToInitiated()
    {
        var aggregate = ConsistencyCheckAggregate.Initiate(NewId("State"), "scope", InitiatedAt);

        Assert.Equal(ConsistencyCheckStatus.Initiated, aggregate.Status);
        Assert.Null(aggregate.HasDiscrepancies);
    }

    [Fact]
    public void Initiate_WithEmptyScopeTarget_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            ConsistencyCheckAggregate.Initiate(NewId("EmptyScope"), "", InitiatedAt));
    }

    [Fact]
    public void Complete_RaisesConsistencyCheckCompletedEvent()
    {
        var aggregate = ConsistencyCheckAggregate.Initiate(NewId("Complete"), "scope", InitiatedAt);
        aggregate.ClearDomainEvents();

        aggregate.Complete(true, CompletedAt);

        var evt = Assert.IsType<ConsistencyCheckCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(evt.HasDiscrepancies);
        Assert.Equal(ConsistencyCheckStatus.Completed, aggregate.Status);
        Assert.True(aggregate.HasDiscrepancies);
    }

    [Fact]
    public void Complete_AlreadyCompleted_Throws()
    {
        var aggregate = ConsistencyCheckAggregate.Initiate(NewId("DoubleComplete"), "scope", InitiatedAt);
        aggregate.Complete(false, CompletedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Complete(true, CompletedAt.AddMinutes(1)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new ConsistencyCheckInitiatedEvent(id, "control-system/audit", InitiatedAt)
        };
        var aggregate = (ConsistencyCheckAggregate)Activator.CreateInstance(typeof(ConsistencyCheckAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ConsistencyCheckStatus.Initiated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
