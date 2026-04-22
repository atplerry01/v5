using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Provider;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Provider;

public sealed class ProviderAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static ProviderId NewId(string seed) =>
        new(IdGen.Generate($"ProviderAggregateTests:{seed}:provider"));

    private static ProviderProfile DefaultProfile() =>
        new(new ClusterRef(IdGen.Generate("ProviderAggregateTests:cluster-ref")), "Alpha Provider");

    [Fact]
    public void Register_RaisesProviderRegisteredEvent()
    {
        var id = NewId("Register_Valid");
        var profile = DefaultProfile();

        var aggregate = ProviderAggregate.Register(id, profile);

        var evt = Assert.IsType<ProviderRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ProviderId);
        Assert.Equal(profile.ProviderName, evt.Profile.ProviderName);
    }

    [Fact]
    public void Register_SetsStatusToRegistered()
    {
        var aggregate = ProviderAggregate.Register(NewId("Register_Status"), DefaultProfile());

        Assert.Equal(ProviderStatus.Registered, aggregate.Status);
    }

    [Fact]
    public void Activate_FromRegistered_RaisesProviderActivatedEvent()
    {
        var aggregate = ProviderAggregate.Register(NewId("Activate_Valid"), DefaultProfile());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<ProviderActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ProviderStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_RaisesProviderSuspendedEvent()
    {
        var aggregate = ProviderAggregate.Register(NewId("Suspend_Valid"), DefaultProfile());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<ProviderSuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ProviderStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Retire_FromActive_RaisesProviderRetiredEvent()
    {
        var aggregate = ProviderAggregate.Register(NewId("Retire_Valid"), DefaultProfile());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<ProviderRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ProviderStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromRegistered_Throws()
    {
        var aggregate = ProviderAggregate.Register(NewId("Suspend_Invalid"), DefaultProfile());

        Assert.ThrowsAny<DomainException>(() => aggregate.Suspend());
    }

    [Fact]
    public void Register_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var p1 = ProviderAggregate.Register(id, DefaultProfile());
        var p2 = ProviderAggregate.Register(id, DefaultProfile());

        Assert.Equal(
            ((ProviderRegisteredEvent)p1.DomainEvents[0]).ProviderId.Value,
            ((ProviderRegisteredEvent)p2.DomainEvents[0]).ProviderId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesProviderState()
    {
        var id = NewId("History");
        var profile = DefaultProfile();

        var history = new object[]
        {
            new ProviderRegisteredEvent(id, profile),
            new ProviderActivatedEvent(id)
        };

        var aggregate = (ProviderAggregate)Activator.CreateInstance(typeof(ProviderAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(ProviderStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
