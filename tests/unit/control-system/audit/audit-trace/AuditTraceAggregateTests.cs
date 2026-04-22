using Whycespace.Domain.ControlSystem.Audit.AuditTrace;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Audit.AuditTrace;

public sealed class AuditTraceAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset OpenedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ClosedAt = new(2026, 4, 22, 11, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static AuditTraceId NewId(string seed) =>
        new(Hex64($"AuditTraceTests:{seed}:trace"));

    [Fact]
    public void Open_RaisesAuditTraceOpenedEvent()
    {
        var id = NewId("Open");

        var aggregate = AuditTraceAggregate.Open(id, "corr-1", OpenedAt);

        var evt = Assert.IsType<AuditTraceOpenedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("corr-1", evt.CorrelationId);
        Assert.Equal(OpenedAt, evt.OpenedAt);
    }

    [Fact]
    public void Open_SetsStatusToOpen()
    {
        var aggregate = AuditTraceAggregate.Open(NewId("State"), "corr-1", OpenedAt);

        Assert.Equal(TraceStatus.Open, aggregate.Status);
        Assert.Empty(aggregate.LinkedAuditEventIds);
    }

    [Fact]
    public void Open_WithEmptyCorrelationId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            AuditTraceAggregate.Open(NewId("EmptyCorr"), "", OpenedAt));
    }

    [Fact]
    public void LinkEvent_RaisesAuditTraceEventLinkedEvent()
    {
        var aggregate = AuditTraceAggregate.Open(NewId("LinkEvent"), "corr-1", OpenedAt);
        aggregate.ClearDomainEvents();

        aggregate.LinkEvent("audit-event-123");

        var evt = Assert.IsType<AuditTraceEventLinkedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("audit-event-123", evt.AuditEventId);
        Assert.Contains("audit-event-123", aggregate.LinkedAuditEventIds);
    }

    [Fact]
    public void LinkEvent_OnClosedTrace_Throws()
    {
        var aggregate = AuditTraceAggregate.Open(NewId("LinkOnClosed"), "corr-1", OpenedAt);
        aggregate.Close(ClosedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.LinkEvent("event-1"));
    }

    [Fact]
    public void Close_RaisesAuditTraceClosedEvent()
    {
        var aggregate = AuditTraceAggregate.Open(NewId("Close"), "corr-1", OpenedAt);
        aggregate.ClearDomainEvents();

        aggregate.Close(ClosedAt);

        var evt = Assert.IsType<AuditTraceClosedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ClosedAt, evt.ClosedAt);
        Assert.Equal(TraceStatus.Closed, aggregate.Status);
    }

    [Fact]
    public void Close_AlreadyClosed_Throws()
    {
        var aggregate = AuditTraceAggregate.Open(NewId("DoubleClose"), "corr-1", OpenedAt);
        aggregate.Close(ClosedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Close(ClosedAt.AddMinutes(5)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new AuditTraceOpenedEvent(id, "corr-1", OpenedAt) };
        var aggregate = (AuditTraceAggregate)Activator.CreateInstance(typeof(AuditTraceAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(TraceStatus.Open, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
