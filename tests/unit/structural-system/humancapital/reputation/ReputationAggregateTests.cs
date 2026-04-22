using Whycespace.Domain.StructuralSystem.Humancapital.Reputation;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Reputation;

public sealed class ReputationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static ReputationId NewId(string seed) =>
        new(IdGen.Generate($"ReputationAggregateTests:{seed}:reputation"));

    private static ReputationDescriptor DefaultDescriptor() =>
        new("Trustworthiness", "Behavioral");

    [Fact]
    public void Create_RaisesReputationCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = ReputationAggregate.Create(id, descriptor);

        var evt = Assert.IsType<ReputationCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ReputationId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = ReputationAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var r1 = ReputationAggregate.Create(id, DefaultDescriptor());
        var r2 = ReputationAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((ReputationCreatedEvent)r1.DomainEvents[0]).ReputationId.Value,
            ((ReputationCreatedEvent)r2.DomainEvents[0]).ReputationId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesReputationState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new ReputationCreatedEvent(id, descriptor)
        };

        var aggregate = (ReputationAggregate)Activator.CreateInstance(typeof(ReputationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
