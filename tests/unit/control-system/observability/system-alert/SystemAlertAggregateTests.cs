using Whycespace.Domain.ControlSystem.Observability.SystemAlert;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Observability.SystemAlert;

public sealed class SystemAlertAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemAlertId NewId(string seed) =>
        new(Hex64($"SystemAlertTests:{seed}:alert"));

    [Fact]
    public void Define_RaisesSystemAlertDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = SystemAlertAggregate.Define(id, "HighCpu", "metric-1", "cpu > 90", AlertSeverity.Critical);

        var evt = Assert.IsType<SystemAlertDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("HighCpu", evt.Name);
        Assert.Equal("metric-1", evt.MetricDefinitionId);
        Assert.Equal("cpu > 90", evt.ConditionExpression);
        Assert.Equal(AlertSeverity.Critical, evt.Severity);
    }

    [Fact]
    public void Define_SetsIsRetiredFalse()
    {
        var aggregate = SystemAlertAggregate.Define(NewId("State"), "Alert", "metric-1", "x > 0", AlertSeverity.Warning);

        Assert.False(aggregate.IsRetired);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemAlertAggregate.Define(NewId("EmptyName"), "", "metric-1", "x > 0", AlertSeverity.Warning));
    }

    [Fact]
    public void Define_WithEmptyMetricDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemAlertAggregate.Define(NewId("EmptyMetric"), "Alert", "", "x > 0", AlertSeverity.Warning));
    }

    [Fact]
    public void Define_WithEmptyConditionExpression_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemAlertAggregate.Define(NewId("EmptyCondition"), "Alert", "metric-1", "", AlertSeverity.Warning));
    }

    [Fact]
    public void Retire_RaisesSystemAlertRetiredEvent()
    {
        var aggregate = SystemAlertAggregate.Define(NewId("Retire"), "Alert", "metric-1", "x > 0", AlertSeverity.Critical);
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<SystemAlertRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsRetired);
    }

    [Fact]
    public void Retire_AlreadyRetired_Throws()
    {
        var aggregate = SystemAlertAggregate.Define(NewId("DoubleRetire"), "Alert", "metric-1", "x > 0", AlertSeverity.Warning);
        aggregate.Retire();

        Assert.ThrowsAny<Exception>(() => aggregate.Retire());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemAlertDefinedEvent(id, "Alert", "metric-1", "x > 0", AlertSeverity.Warning)
        };
        var aggregate = (SystemAlertAggregate)Activator.CreateInstance(typeof(SystemAlertAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("Alert", aggregate.Name);
        Assert.False(aggregate.IsRetired);
        Assert.Empty(aggregate.DomainEvents);
    }
}
