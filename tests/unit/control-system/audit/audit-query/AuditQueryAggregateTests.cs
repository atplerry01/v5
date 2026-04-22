using Whycespace.Domain.ControlSystem.Audit.AuditQuery;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Audit.AuditQuery;

public sealed class AuditQueryAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset From = new(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset To = new(2026, 4, 22, 0, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AuditQueryId NewId(string seed) =>
        new(Hex64($"AuditQueryTests:{seed}:query"));

    private static QueryTimeRange DefaultRange() => new(From, To);

    [Fact]
    public void Issue_RaisesAuditQueryIssuedEvent()
    {
        var id = NewId("Issue");

        var aggregate = AuditQueryAggregate.Issue(id, "auditor-1", DefaultRange());

        var evt = Assert.IsType<AuditQueryIssuedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("auditor-1", evt.IssuedBy);
        Assert.Equal(From, evt.TimeRange.From);
        Assert.Equal(To, evt.TimeRange.To);
    }

    [Fact]
    public void Issue_SetsStatusToIssued()
    {
        var aggregate = AuditQueryAggregate.Issue(NewId("State"), "auditor-1", DefaultRange());

        Assert.Equal(AuditQueryStatus.Issued, aggregate.Status);
        Assert.Null(aggregate.ResultCount);
    }

    [Fact]
    public void Issue_WithEmptyIssuedBy_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditQueryAggregate.Issue(NewId("EmptyIssuedBy"), "", DefaultRange()));
    }

    [Fact]
    public void Issue_WithFilters_SetsFilters()
    {
        var aggregate = AuditQueryAggregate.Issue(
            NewId("Filters"), "auditor-1", DefaultRange(), "corr-filter", "actor-filter");

        Assert.Equal("corr-filter", aggregate.CorrelationFilter);
        Assert.Equal("actor-filter", aggregate.ActorFilter);
    }

    [Fact]
    public void Complete_RaisesAuditQueryCompletedEvent()
    {
        var aggregate = AuditQueryAggregate.Issue(NewId("Complete"), "auditor-1", DefaultRange());
        aggregate.ClearDomainEvents();

        aggregate.Complete(42);

        var evt = Assert.IsType<AuditQueryCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(42, evt.ResultCount);
        Assert.Equal(AuditQueryStatus.Completed, aggregate.Status);
        Assert.Equal(42, aggregate.ResultCount);
    }

    [Fact]
    public void Expire_RaisesAuditQueryExpiredEvent()
    {
        var aggregate = AuditQueryAggregate.Issue(NewId("Expire"), "auditor-1", DefaultRange());
        aggregate.ClearDomainEvents();

        aggregate.Expire();

        Assert.IsType<AuditQueryExpiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AuditQueryStatus.Expired, aggregate.Status);
    }

    [Fact]
    public void Complete_AlreadyCompleted_Throws()
    {
        var aggregate = AuditQueryAggregate.Issue(NewId("DoubleComplete"), "auditor-1", DefaultRange());
        aggregate.Complete(10);

        Assert.ThrowsAny<Exception>(() => aggregate.Complete(20));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new AuditQueryIssuedEvent(id, "auditor-1", DefaultRange(), null, null) };
        var aggregate = (AuditQueryAggregate)Activator.CreateInstance(typeof(AuditQueryAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AuditQueryStatus.Issued, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
