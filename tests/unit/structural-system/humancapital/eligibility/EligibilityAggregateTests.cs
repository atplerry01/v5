using Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Humancapital.Eligibility;

public sealed class EligibilityAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static EligibilityId NewId(string seed) =>
        new(IdGen.Generate($"EligibilityAggregateTests:{seed}:eligibility"));

    private static EligibilityDescriptor DefaultDescriptor() =>
        new("Standard Eligibility", "Participation");

    [Fact]
    public void Create_RaisesEligibilityCreatedEvent()
    {
        var id = NewId("Create_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = EligibilityAggregate.Create(id, descriptor);

        var evt = Assert.IsType<EligibilityCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.EligibilityId);
        Assert.Equal(descriptor.Name, evt.Descriptor.Name);
        Assert.Equal(descriptor.Kind, evt.Descriptor.Kind);
    }

    [Fact]
    public void Create_SetsIdentityAndDescriptor()
    {
        var id = NewId("Create_State");
        var descriptor = DefaultDescriptor();

        var aggregate = EligibilityAggregate.Create(id, descriptor);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
    }

    [Fact]
    public void Create_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var e1 = EligibilityAggregate.Create(id, DefaultDescriptor());
        var e2 = EligibilityAggregate.Create(id, DefaultDescriptor());

        Assert.Equal(
            ((EligibilityCreatedEvent)e1.DomainEvents[0]).EligibilityId.Value,
            ((EligibilityCreatedEvent)e2.DomainEvents[0]).EligibilityId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesEligibilityState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new EligibilityCreatedEvent(id, descriptor)
        };

        var aggregate = (EligibilityAggregate)Activator.CreateInstance(typeof(EligibilityAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor, aggregate.Descriptor);
        Assert.Empty(aggregate.DomainEvents);
    }
}
