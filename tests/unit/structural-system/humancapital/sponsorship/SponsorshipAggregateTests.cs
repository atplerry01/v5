using Whycespace.Domain.StructuralSystem.Humancapital.Sponsorship;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Sponsorship;

public sealed class SponsorshipAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static SponsorshipId NewId(string seed) =>
        new(IdGen.Generate($"SponsorshipAggregateTests:{seed}:sponsorship"));

    private static SponsorshipDescriptor DefaultDescriptor() =>
        new("Cluster Sponsorship", "FormalBinding");

    [Fact]
    public void Create_RaisesSponsorshipCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = SponsorshipAggregate.Create(id, descriptor);

        var evt = Assert.IsType<SponsorshipCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SponsorshipId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = SponsorshipAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var s1 = SponsorshipAggregate.Create(id, DefaultDescriptor());
        var s2 = SponsorshipAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((SponsorshipCreatedEvent)s1.DomainEvents[0]).SponsorshipId.Value,
            ((SponsorshipCreatedEvent)s2.DomainEvents[0]).SponsorshipId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesSponsorshipState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new SponsorshipCreatedEvent(id, descriptor)
        };

        var aggregate = (SponsorshipAggregate)Activator.CreateInstance(typeof(SponsorshipAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
