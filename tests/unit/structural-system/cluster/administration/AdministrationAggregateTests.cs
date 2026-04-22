using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Administration;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Administration;

public sealed class AdministrationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static AdministrationId NewId(string seed) =>
        new(IdGen.Generate($"AdministrationAggregateTests:{seed}:administration"));

    private static AdministrationDescriptor DefaultDescriptor() =>
        new(new ClusterRef(IdGen.Generate("AdministrationAggregateTests:cluster-ref")), "Alpha Administration");

    [Fact]
    public void Establish_RaisesAdministrationEstablishedEvent()
    {
        var id = NewId("Establish_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = AdministrationAggregate.Establish(id, descriptor);

        var evt = Assert.IsType<AdministrationEstablishedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.AdministrationId);
        Assert.Equal(descriptor.AdministrationName, evt.Descriptor.AdministrationName);
    }

    [Fact]
    public void Establish_SetsStatusToEstablished()
    {
        var aggregate = AdministrationAggregate.Establish(NewId("Establish_Status"), DefaultDescriptor());

        Assert.Equal(AdministrationStatus.Established, aggregate.Status);
    }

    [Fact]
    public void Activate_FromEstablished_RaisesAdministrationActivatedEvent()
    {
        var aggregate = AdministrationAggregate.Establish(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<AdministrationActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AdministrationStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_RaisesAdministrationSuspendedEvent()
    {
        var aggregate = AdministrationAggregate.Establish(NewId("Suspend_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<AdministrationSuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AdministrationStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Retire_FromActive_RaisesAdministrationRetiredEvent()
    {
        var aggregate = AdministrationAggregate.Establish(NewId("Retire_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<AdministrationRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AdministrationStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromEstablished_Throws()
    {
        var aggregate = AdministrationAggregate.Establish(NewId("Suspend_Invalid"), DefaultDescriptor());

        Assert.ThrowsAny<DomainException>(() => aggregate.Suspend());
    }

    [Fact]
    public void Establish_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var a1 = AdministrationAggregate.Establish(id, DefaultDescriptor());
        var a2 = AdministrationAggregate.Establish(id, DefaultDescriptor());

        Assert.Equal(
            ((AdministrationEstablishedEvent)a1.DomainEvents[0]).AdministrationId.Value,
            ((AdministrationEstablishedEvent)a2.DomainEvents[0]).AdministrationId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesAdministrationState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new AdministrationEstablishedEvent(id, descriptor),
            new AdministrationActivatedEvent(id)
        };

        var aggregate = (AdministrationAggregate)Activator.CreateInstance(typeof(AdministrationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AdministrationStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
