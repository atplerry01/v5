using Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Routing;

public sealed class RouteDefinitionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute Source = new("economic", "capital", "account");
    private static readonly DomainRoute Destination = new("platform", "routing", "dispatch-rule");

    private static RouteDefinitionAggregate NewRegistered(string seed)
    {
        var id = new RouteDefinitionId(IdGen.Generate($"RouteDefinitionAggregateTests:{seed}"));
        return RouteDefinitionAggregate.Register(id, "CapitalToDispatch", Source, Destination, TransportHint.Kafka, 5, Now);
    }

    [Fact]
    public void Register_WithValidArgs_RaisesRouteDefinitionRegisteredEvent()
    {
        var aggregate = NewRegistered("Register");

        Assert.Equal(RouteDefinitionStatus.Active, aggregate.Status);
        Assert.Equal("CapitalToDispatch", aggregate.RouteName);
        Assert.Equal(Source, aggregate.SourceRoute);
        Assert.Equal(Destination, aggregate.DestinationRoute);
        Assert.Equal(TransportHint.Kafka, aggregate.TransportHint);
        Assert.Equal(5, aggregate.Priority);

        var evt = Assert.IsType<RouteDefinitionRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("Kafka", evt.TransportHint.Value);
    }

    [Fact]
    public void Deactivate_FromActive_TransitionsToInactive()
    {
        var aggregate = NewRegistered("Deactivate");
        aggregate.ClearDomainEvents();

        aggregate.Deactivate(Now);

        Assert.Equal(RouteDefinitionStatus.Inactive, aggregate.Status);
        Assert.IsType<RouteDefinitionDeactivatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deprecate_FromActive_TransitionsToDeprecated()
    {
        var aggregate = NewRegistered("Deprecate");
        aggregate.ClearDomainEvents();

        aggregate.Deprecate(Now);

        Assert.Equal(RouteDefinitionStatus.Deprecated, aggregate.Status);
        Assert.IsType<RouteDefinitionDeprecatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Throws()
    {
        var aggregate = NewRegistered("DoubleDeactivate");
        aggregate.Deactivate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deactivate(Now));
    }

    [Fact]
    public void Deactivate_WhenDeprecated_Throws()
    {
        var aggregate = NewRegistered("DeactivateFromDeprecated");
        aggregate.Deprecate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deactivate(Now));
    }

    [Fact]
    public void Deprecate_WhenAlreadyDeprecated_Throws()
    {
        var aggregate = NewRegistered("DoubleDeprecate");
        aggregate.Deprecate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deprecate(Now));
    }

    [Fact]
    public void Register_WithSelfRouting_Throws()
    {
        var id = new RouteDefinitionId(IdGen.Generate("RouteDefinitionAggregateTests:SelfRoute"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDefinitionAggregate.Register(id, "SelfRoute", Source, Source, TransportHint.Kafka, 1, Now));
    }

    [Fact]
    public void Register_WithEmptyRouteName_Throws()
    {
        var id = new RouteDefinitionId(IdGen.Generate("RouteDefinitionAggregateTests:EmptyName"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDefinitionAggregate.Register(id, "", Source, Destination, TransportHint.Kafka, 1, Now));
    }

    [Fact]
    public void Register_WithNegativePriority_Throws()
    {
        var id = new RouteDefinitionId(IdGen.Generate("RouteDefinitionAggregateTests:NegPriority"));

        Assert.Throws<DomainInvariantViolationException>(() =>
            RouteDefinitionAggregate.Register(id, "Route", Source, Destination, TransportHint.Kafka, -1, Now));
    }
}
