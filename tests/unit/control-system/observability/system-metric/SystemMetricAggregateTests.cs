using Whycespace.Domain.ControlSystem.Observability.SystemMetric;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Observability.SystemMetric;

public sealed class SystemMetricAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemMetricId NewId(string seed) =>
        new(Hex64($"SystemMetricTests:{seed}:metric"));

    [Fact]
    public void Define_RaisesSystemMetricDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = SystemMetricAggregate.Define(id, "cpu_usage", MetricType.Gauge, "percent", ["host", "region"]);

        var evt = Assert.IsType<SystemMetricDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("cpu_usage", evt.Name);
        Assert.Equal(MetricType.Gauge, evt.Type);
        Assert.Equal("percent", evt.Unit);
        Assert.Equal(2, evt.LabelNames.Count);
    }

    [Fact]
    public void Define_SetsIsDeprecatedFalse()
    {
        var aggregate = SystemMetricAggregate.Define(NewId("State"), "requests", MetricType.Counter, "count", []);

        Assert.False(aggregate.IsDeprecated);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemMetricAggregate.Define(NewId("EmptyName"), "", MetricType.Counter, "count", []));
    }

    [Fact]
    public void Define_WithEmptyUnit_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemMetricAggregate.Define(NewId("EmptyUnit"), "metric", MetricType.Counter, "", []));
    }

    [Fact]
    public void Deprecate_RaisesSystemMetricDeprecatedEvent()
    {
        var aggregate = SystemMetricAggregate.Define(NewId("Deprecate"), "metric", MetricType.Histogram, "ms", []);
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<SystemMetricDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsDeprecated);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = SystemMetricAggregate.Define(NewId("DoubleDeprecate"), "metric", MetricType.Counter, "count", []);
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemMetricDefinedEvent(id, "cpu_usage", MetricType.Gauge, "percent", ["host"])
        };
        var aggregate = (SystemMetricAggregate)Activator.CreateInstance(typeof(SystemMetricAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("cpu_usage", aggregate.Name);
        Assert.False(aggregate.IsDeprecated);
        Assert.Empty(aggregate.DomainEvents);
    }
}
