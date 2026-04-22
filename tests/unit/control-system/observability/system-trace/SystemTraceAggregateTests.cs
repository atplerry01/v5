using Whycespace.Domain.ControlSystem.Observability.SystemTrace;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Observability.SystemTrace;

public sealed class SystemTraceAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset StartedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CompletedAt = new(2026, 4, 22, 10, 0, 5, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemTraceId NewId(string seed) =>
        new(Hex64($"SystemTraceTests:{seed}:trace"));

    [Fact]
    public void Start_RaisesSystemTraceSpanStartedEvent()
    {
        var id = NewId("Start");

        var aggregate = SystemTraceAggregate.Start(id, "trace-abc", "GET /api/items", StartedAt);

        var evt = Assert.IsType<SystemTraceSpanStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("trace-abc", evt.TraceId);
        Assert.Equal("GET /api/items", evt.OperationName);
        Assert.Null(evt.ParentSpanId);
    }

    [Fact]
    public void Start_WithParentSpanId_SetsParentSpanId()
    {
        var aggregate = SystemTraceAggregate.Start(NewId("Parent"), "trace-abc", "child-op", StartedAt, "parent-span-1");

        Assert.Equal("parent-span-1", aggregate.ParentSpanId);
    }

    [Fact]
    public void Start_WithEmptyTraceId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemTraceAggregate.Start(NewId("EmptyTrace"), "", "op", StartedAt));
    }

    [Fact]
    public void Start_WithEmptyOperationName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemTraceAggregate.Start(NewId("EmptyOp"), "trace-abc", "", StartedAt));
    }

    [Fact]
    public void Complete_RaisesSystemTraceSpanCompletedEvent()
    {
        var aggregate = SystemTraceAggregate.Start(NewId("Complete"), "trace-abc", "op", StartedAt);
        aggregate.ClearDomainEvents();

        aggregate.Complete(CompletedAt, SpanStatus.Success);

        var evt = Assert.IsType<SystemTraceSpanCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(CompletedAt, evt.CompletedAt);
        Assert.Equal(SpanStatus.Success, evt.Status);
        Assert.Equal(SpanStatus.Success, aggregate.Status);
    }

    [Fact]
    public void Complete_WithCompletedAtBeforeStartedAt_Throws()
    {
        var aggregate = SystemTraceAggregate.Start(NewId("BeforeStart"), "trace-abc", "op", StartedAt);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.Complete(StartedAt.AddSeconds(-1), SpanStatus.Error));
    }

    [Fact]
    public void Complete_AlreadyCompleted_Throws()
    {
        var aggregate = SystemTraceAggregate.Start(NewId("DoubleComplete"), "trace-abc", "op", StartedAt);
        aggregate.Complete(CompletedAt, SpanStatus.Success);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.Complete(CompletedAt.AddSeconds(1), SpanStatus.Error));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemTraceSpanStartedEvent(id, "trace-abc", "GET /api/items", StartedAt, null)
        };
        var aggregate = (SystemTraceAggregate)Activator.CreateInstance(typeof(SystemTraceAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("trace-abc", aggregate.TraceId);
        Assert.Null(aggregate.CompletedAt);
        Assert.Empty(aggregate.DomainEvents);
    }
}
