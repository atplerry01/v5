using Whycespace.Domain.ControlSystem.Audit.AuditRecord;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Audit.AuditRecord;

public sealed class AuditRecordAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ResolvedTime = new(2026, 4, 22, 12, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AuditRecordId NewId(string seed) =>
        new(Hex64($"AuditRecordTests:{seed}:record"));

    private static IReadOnlyList<string> DefaultEntryIds() => ["entry-1", "entry-2"];

    [Fact]
    public void Raise_RaisesAuditRecordRaisedEvent()
    {
        var id = NewId("Raise");

        var aggregate = AuditRecordAggregate.Raise(id, DefaultEntryIds(), "Policy violated", AuditRecordSeverity.Violation, BaseTime);

        var evt = Assert.IsType<AuditRecordRaisedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal(AuditRecordSeverity.Violation, evt.Severity);
        Assert.Equal(2, evt.AuditLogEntryIds.Count);
    }

    [Fact]
    public void Raise_SetsStatusToOpen()
    {
        var aggregate = AuditRecordAggregate.Raise(NewId("State"), DefaultEntryIds(), "desc", AuditRecordSeverity.Warning, BaseTime);

        Assert.Equal(AuditRecordStatus.Open, aggregate.Status);
    }

    [Fact]
    public void Raise_WithNoEntryIds_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditRecordAggregate.Raise(NewId("NoEntries"), [], "desc", AuditRecordSeverity.Informational, BaseTime));
    }

    [Fact]
    public void Raise_WithEmptyDescription_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditRecordAggregate.Raise(NewId("EmptyDesc"), DefaultEntryIds(), "", AuditRecordSeverity.Informational, BaseTime));
    }

    [Fact]
    public void Resolve_RaisesAuditRecordResolvedEvent()
    {
        var aggregate = AuditRecordAggregate.Raise(NewId("Resolve"), DefaultEntryIds(), "desc", AuditRecordSeverity.Critical, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Resolve(ResolvedTime);

        var evt = Assert.IsType<AuditRecordResolvedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ResolvedTime, evt.ResolvedAt);
        Assert.Equal(AuditRecordStatus.Resolved, aggregate.Status);
        Assert.Equal(ResolvedTime, aggregate.ResolvedAt);
    }

    [Fact]
    public void Resolve_AlreadyResolved_Throws()
    {
        var aggregate = AuditRecordAggregate.Raise(NewId("DoubleResolve"), DefaultEntryIds(), "desc", AuditRecordSeverity.Warning, BaseTime);
        aggregate.Resolve(ResolvedTime);

        Assert.ThrowsAny<Exception>(() => aggregate.Resolve(ResolvedTime.AddMinutes(5)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new AuditRecordRaisedEvent(id, DefaultEntryIds(), "desc", AuditRecordSeverity.Warning, BaseTime)
        };
        var aggregate = (AuditRecordAggregate)Activator.CreateInstance(typeof(AuditRecordAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AuditRecordStatus.Open, aggregate.Status);
        Assert.Equal(2, aggregate.AuditLogEntryIds.Count);
        Assert.Empty(aggregate.DomainEvents);
    }
}
