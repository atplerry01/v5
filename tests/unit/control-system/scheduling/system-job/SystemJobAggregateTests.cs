using Whycespace.Domain.ControlSystem.Scheduling.SystemJob;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.Scheduling.SystemJob;

public sealed class SystemJobAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static SystemJobId NewId(string seed) =>
        new(Hex64($"SystemJobTests:{seed}:job"));

    [Fact]
    public void Define_RaisesSystemJobDefinedEvent()
    {
        var id = NewId("Define");

        var aggregate = SystemJobAggregate.Define(id, "AuditSweep", JobCategory.Audit, TimeSpan.FromMinutes(30));

        var evt = Assert.IsType<SystemJobDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("AuditSweep", evt.Name);
        Assert.Equal(JobCategory.Audit, evt.Category);
        Assert.Equal(TimeSpan.FromMinutes(30), evt.Timeout);
    }

    [Fact]
    public void Define_SetsIsDeprecatedFalse()
    {
        var aggregate = SystemJobAggregate.Define(NewId("State"), "Job", JobCategory.Maintenance, TimeSpan.FromMinutes(5));

        Assert.False(aggregate.IsDeprecated);
    }

    [Fact]
    public void Define_WithEmptyName_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemJobAggregate.Define(NewId("EmptyName"), "", JobCategory.Diagnostic, TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void Define_WithZeroTimeout_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemJobAggregate.Define(NewId("ZeroTimeout"), "Job", JobCategory.Reconciliation, TimeSpan.Zero));
    }

    [Fact]
    public void Define_WithNegativeTimeout_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            SystemJobAggregate.Define(NewId("NegativeTimeout"), "Job", JobCategory.Maintenance, TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Deprecate_RaisesSystemJobDeprecatedEvent()
    {
        var aggregate = SystemJobAggregate.Define(NewId("Deprecate"), "Job", JobCategory.Audit, TimeSpan.FromMinutes(10));
        aggregate.ClearDomainEvents();

        aggregate.Deprecate();

        Assert.IsType<SystemJobDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.True(aggregate.IsDeprecated);
    }

    [Fact]
    public void Deprecate_AlreadyDeprecated_Throws()
    {
        var aggregate = SystemJobAggregate.Define(NewId("DoubleDeprecate"), "Job", JobCategory.Diagnostic, TimeSpan.FromMinutes(5));
        aggregate.Deprecate();

        Assert.ThrowsAny<Exception>(() => aggregate.Deprecate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new SystemJobDefinedEvent(id, "AuditSweep", JobCategory.Audit, TimeSpan.FromMinutes(30))
        };
        var aggregate = (SystemJobAggregate)Activator.CreateInstance(typeof(SystemJobAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal("AuditSweep", aggregate.Name);
        Assert.False(aggregate.IsDeprecated);
        Assert.Empty(aggregate.DomainEvents);
    }
}
