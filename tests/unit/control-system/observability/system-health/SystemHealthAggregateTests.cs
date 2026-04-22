using Whycespace.Domain.ControlSystem.Observability.SystemHealth;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Observability.SystemHealth;

public sealed class SystemHealthAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemHealthId NewId(string seed) =>
        new(Hex64($"SystemHealthTests:{seed}:health"));

    [Fact]
    public void Evaluate_RaisesSystemHealthEvaluatedEvent()
    {
        var id = NewId("Evaluate");

        var aggregate = SystemHealthAggregate.Evaluate(id, "api-service", HealthStatus.Healthy, BaseTime);

        var evt = Assert.IsType<SystemHealthEvaluatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("api-service", evt.ComponentName);
        Assert.Equal(HealthStatus.Healthy, evt.Status);
    }

    [Fact]
    public void Evaluate_WithEmptyComponentName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemHealthAggregate.Evaluate(NewId("EmptyComp"), "", HealthStatus.Healthy, BaseTime));
    }

    [Fact]
    public void RecordDegradation_RaisesSystemHealthDegradedEvent()
    {
        var aggregate = SystemHealthAggregate.Evaluate(NewId("Degrade"), "api-service", HealthStatus.Healthy, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.RecordDegradation(HealthStatus.Degraded, "high latency", BaseTime.AddMinutes(5));

        var evt = Assert.IsType<SystemHealthDegradedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(HealthStatus.Degraded, evt.NewStatus);
        Assert.Equal("high latency", evt.Reason);
        Assert.Equal(HealthStatus.Degraded, aggregate.Status);
    }

    [Fact]
    public void RecordDegradation_WithSameStatus_Throws()
    {
        var aggregate = SystemHealthAggregate.Evaluate(NewId("SameStatus"), "api-service", HealthStatus.Degraded, BaseTime);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.RecordDegradation(HealthStatus.Degraded, "reason", BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void RecordDegradation_WithHealthyStatus_Throws()
    {
        var aggregate = SystemHealthAggregate.Evaluate(NewId("HealthyDeg"), "api-service", HealthStatus.Degraded, BaseTime);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.RecordDegradation(HealthStatus.Healthy, "reason", BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void RecordDegradation_WithEmptyReason_Throws()
    {
        var aggregate = SystemHealthAggregate.Evaluate(NewId("EmptyReason"), "api-service", HealthStatus.Healthy, BaseTime);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.RecordDegradation(HealthStatus.Degraded, "", BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void Restore_RaisesSystemHealthRestoredEvent()
    {
        var aggregate = SystemHealthAggregate.Evaluate(NewId("Restore"), "api-service", HealthStatus.Degraded, BaseTime);
        aggregate.ClearDomainEvents();

        aggregate.Restore(BaseTime.AddMinutes(10));

        Assert.IsType<SystemHealthRestoredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(HealthStatus.Healthy, aggregate.Status);
    }

    [Fact]
    public void Restore_WhenAlreadyHealthy_Throws()
    {
        var aggregate = SystemHealthAggregate.Evaluate(NewId("RestoreHealthy"), "api-service", HealthStatus.Healthy, BaseTime);

        Assert.ThrowsAny<Exception>(() => aggregate.Restore(BaseTime.AddMinutes(1)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemHealthEvaluatedEvent(id, "api-service", HealthStatus.Healthy, BaseTime)
        };
        var aggregate = (SystemHealthAggregate)Activator.CreateInstance(typeof(SystemHealthAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(HealthStatus.Healthy, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
