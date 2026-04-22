using Whycespace.Domain.ControlSystem.Observability.SystemSignal;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Observability.SystemSignal;

public sealed class SystemSignalAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemSignalId NewId(string seed) =>
        new(Hex64($"SystemSignalTests:{seed}:signal"));

    [Fact]
    public void Define_RaisesSystemSignalDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = SystemSignalAggregate.Define(id, "HeartbeatSignal", SignalKind.Heartbeat, "api-service");

        var evt = Assert.IsType<SystemSignalDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("HeartbeatSignal", evt.Name);
        Assert.Equal(SignalKind.Heartbeat, evt.Kind);
        Assert.Equal("api-service", evt.Source);
    }

    [Fact]
    public void Define_SetsIsDeprecatedFalse()
    {
        var aggregate = SystemSignalAggregate.Define(NewId("State"), "Signal", SignalKind.Anomaly, "source-1");

        Assert.False(aggregate.IsDeprecated);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemSignalAggregate.Define(NewId("EmptyName"), "", SignalKind.Threshold, "source-1"));
    }

    [Fact]
    public void Define_WithEmptySource_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemSignalAggregate.Define(NewId("EmptySource"), "Signal", SignalKind.Threshold, ""));
    }

    [Fact]
    public void Deprecate_RaisesSystemSignalDeprecatedEvent()
    {
        var aggregate = SystemSignalAggregate.Define(NewId("Deprecate"), "Signal", SignalKind.Recovery, "source-1");
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<SystemSignalDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsDeprecated);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = SystemSignalAggregate.Define(NewId("DoubleDeprecate"), "Signal", SignalKind.Degradation, "source-1");
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemSignalDefinedEvent(id, "HeartbeatSignal", SignalKind.Heartbeat, "api-service")
        };
        var aggregate = (SystemSignalAggregate)Activator.CreateInstance(typeof(SystemSignalAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("HeartbeatSignal", aggregate.Name);
        Assert.False(aggregate.IsDeprecated);
        Assert.Empty(aggregate.DomainEvents);
    }
}
