using Whycespace.Domain.PlatformSystem.Schema.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Schema;

public sealed class ContractAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute Publisher = new("economic", "capital", "account");
    private static readonly Guid SomeSchemaRef = new("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private static ContractAggregate NewRegistered(string seed)
    {
        var id = new ContractId(IdGen.Generate($"ContractAggregateTests:{seed}"));
        return ContractAggregate.Register(id, "AccountContract", Publisher, SomeSchemaRef, 1, Now);
    }

    [Fact]
    public void Register_WithValidArgs_RaisesContractRegisteredEvent()
    {
        var aggregate = NewRegistered("Register");

        Assert.Equal(ContractStatus.Active, aggregate.Status);
        Assert.Equal("AccountContract", aggregate.ContractName);
        Assert.Equal(SomeSchemaRef, aggregate.SchemaRef);
        Assert.Empty(aggregate.SubscriberConstraints);

        Assert.IsType<ContractRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void AddSubscriber_FromActive_AddsConstraintAndRaisesEvent()
    {
        var aggregate = NewRegistered("AddSubscriber");
        aggregate.ClearDomainEvents();

        var subscriberRoute = new DomainRoute("platform", "routing", "dispatch-rule");
        var constraint = new SubscriberConstraint(subscriberRoute, 1, ContractCompatibilityMode.Backward);
        aggregate.AddSubscriber(constraint, Now);

        Assert.Single(aggregate.SubscriberConstraints);
        Assert.Equal(subscriberRoute, aggregate.SubscriberConstraints[0].SubscriberRoute);

        var evt = Assert.IsType<ContractSubscriberAddedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("Backward", evt.Constraint.RequiredCompatibilityMode.Value);
    }

    [Fact]
    public void AddSubscriber_MultipleTimes_AccumulatesConstraints()
    {
        var aggregate = NewRegistered("MultipleSubscribers");

        var route1 = new DomainRoute("platform", "routing", "dispatch-rule");
        var route2 = new DomainRoute("economic", "capital", "reserve");
        aggregate.AddSubscriber(new SubscriberConstraint(route1, 1, ContractCompatibilityMode.Backward), Now);
        aggregate.AddSubscriber(new SubscriberConstraint(route2, 2, ContractCompatibilityMode.Forward), Now);

        Assert.Equal(2, aggregate.SubscriberConstraints.Count);
    }

    [Fact]
    public void Deprecate_FromActive_TransitionsToDeprecated()
    {
        var aggregate = NewRegistered("Deprecate");
        aggregate.ClearDomainEvents();

        aggregate.Deprecate(Now);

        Assert.Equal(ContractStatus.Deprecated, aggregate.Status);
        Assert.IsType<ContractDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void AddSubscriber_WhenDeprecated_Throws()
    {
        var aggregate = NewRegistered("AddSubscriberDeprecated");
        aggregate.Deprecate(Now);

        var constraint = new SubscriberConstraint(new DomainRoute("a", "b", "c"), 1, ContractCompatibilityMode.None);
        Assert.Throws<DomainInvariantViolationException>(() => aggregate.AddSubscriber(constraint, Now));
    }

    [Fact]
    public void Deprecate_WhenAlreadyDeprecated_Throws()
    {
        var aggregate = NewRegistered("DoubleDeprecate");
        aggregate.Deprecate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deprecate(Now));
    }

    [Fact]
    public void Register_WithEmptyContractName_Throws()
    {
        var id = new ContractId(IdGen.Generate("ContractAggregateTests:EmptyName"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            ContractAggregate.Register(id, "", Publisher, SomeSchemaRef, 1, Now));
    }

    [Fact]
    public void Register_WithEmptySchemaRef_Throws()
    {
        var id = new ContractId(IdGen.Generate("ContractAggregateTests:EmptySchema"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            ContractAggregate.Register(id, "Contract", Publisher, Guid.Empty, 1, Now));
    }
}
