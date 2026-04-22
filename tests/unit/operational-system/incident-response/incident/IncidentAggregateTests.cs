using Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static IncidentId NewId(string seed) =>
        new(IdGen.Generate($"IncidentAggregateTests:{seed}:incident"));

    private static IncidentDescriptor DefaultDescriptor() =>
        new("Database primary node unresponsive", "P1");

    [Fact]
    public void Report_RaisesIncidentReportedEvent()
    {
        var id = NewId("Report_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = IncidentAggregate.Report(id, descriptor);

        var evt = Assert.IsType<IncidentReportedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.IncidentId);
        Assert.Equal(descriptor, evt.Descriptor);
    }

    [Fact]
    public void Report_SetsStatusToReported()
    {
        var id = NewId("Report_State");

        var aggregate = IncidentAggregate.Report(id, DefaultDescriptor());

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(IncidentStatus.Reported, aggregate.Status);
    }

    [Fact]
    public void Report_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var descriptor = DefaultDescriptor();
        var i1 = IncidentAggregate.Report(id, descriptor);
        var i2 = IncidentAggregate.Report(id, descriptor);

        Assert.Equal(
            ((IncidentReportedEvent)i1.DomainEvents[0]).IncidentId.Value,
            ((IncidentReportedEvent)i2.DomainEvents[0]).IncidentId.Value);
    }

    [Fact]
    public void Investigate_FromReported_RaisesInvestigationStartedEvent()
    {
        var id = NewId("Investigate_Valid");
        var aggregate = IncidentAggregate.Report(id, DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Investigate();

        Assert.IsType<IncidentInvestigationStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IncidentStatus.Investigating, aggregate.Status);
    }

    [Fact]
    public void Resolve_FromInvestigating_RaisesIncidentResolvedEvent()
    {
        var id = NewId("Resolve_Valid");
        var aggregate = IncidentAggregate.Report(id, DefaultDescriptor());
        aggregate.Investigate();
        aggregate.ClearDomainEvents();

        aggregate.Resolve();

        Assert.IsType<IncidentResolvedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IncidentStatus.Resolved, aggregate.Status);
    }

    [Fact]
    public void Close_FromResolved_RaisesIncidentClosedEvent()
    {
        var id = NewId("Close_Valid");
        var aggregate = IncidentAggregate.Report(id, DefaultDescriptor());
        aggregate.Investigate();
        aggregate.Resolve();
        aggregate.ClearDomainEvents();

        aggregate.Close();

        Assert.IsType<IncidentClosedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IncidentStatus.Closed, aggregate.Status);
    }

    [Fact]
    public void Investigate_FromClosed_Throws()
    {
        var id = NewId("Investigate_FromClosed");
        var aggregate = IncidentAggregate.Report(id, DefaultDescriptor());
        aggregate.Investigate();
        aggregate.Resolve();
        aggregate.Close();

        Assert.ThrowsAny<Exception>(() => aggregate.Investigate());
    }

    [Fact]
    public void Resolve_FromReported_Throws()
    {
        var id = NewId("Resolve_FromReported");
        var aggregate = IncidentAggregate.Report(id, DefaultDescriptor());

        Assert.ThrowsAny<Exception>(() => aggregate.Resolve());
    }

    [Fact]
    public void LoadFromHistory_RehydratesIncidentState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new IncidentReportedEvent(id, descriptor)
        };

        var aggregate = (IncidentAggregate)Activator.CreateInstance(typeof(IncidentAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(IncidentStatus.Reported, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
