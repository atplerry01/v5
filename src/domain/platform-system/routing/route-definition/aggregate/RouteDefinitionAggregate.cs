using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public sealed class RouteDefinitionAggregate : AggregateRoot
{
    public RouteDefinitionId RouteDefinitionId { get; private set; }
    public string RouteName { get; private set; } = string.Empty;
    public DomainRoute SourceRoute { get; private set; } = null!;
    public DomainRoute DestinationRoute { get; private set; } = null!;
    public TransportHint TransportHint { get; private set; }
    public int Priority { get; private set; }
    public RouteDefinitionStatus Status { get; private set; }

    private RouteDefinitionAggregate() { }

    public static RouteDefinitionAggregate Register(
        RouteDefinitionId id,
        string routeName,
        DomainRoute sourceRoute,
        DomainRoute destinationRoute,
        TransportHint transportHint,
        int priority,
        Timestamp registeredAt)
    {
        var aggregate = new RouteDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw RouteDefinitionErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(routeName))
            throw RouteDefinitionErrors.RouteNameMissing();

        if (!sourceRoute.IsValid())
            throw RouteDefinitionErrors.InvalidSourceRoute();

        if (!destinationRoute.IsValid())
            throw RouteDefinitionErrors.InvalidDestinationRoute();

        if (sourceRoute == destinationRoute)
            throw RouteDefinitionErrors.SelfRoutingForbidden();

        if (priority < 0)
            throw RouteDefinitionErrors.PriorityNegative();

        aggregate.RaiseDomainEvent(new RouteDefinitionRegisteredEvent(
            id, routeName, sourceRoute, destinationRoute, transportHint, priority, registeredAt));

        return aggregate;
    }

    public void Deactivate(Timestamp deactivatedAt)
    {
        if (Status == RouteDefinitionStatus.Deprecated)
            throw RouteDefinitionErrors.AlreadyDeprecated();

        if (Status == RouteDefinitionStatus.Inactive)
            throw RouteDefinitionErrors.AlreadyInactive();

        RaiseDomainEvent(new RouteDefinitionDeactivatedEvent(RouteDefinitionId, deactivatedAt));
    }

    public void Deprecate(Timestamp deprecatedAt)
    {
        if (Status == RouteDefinitionStatus.Deprecated)
            throw RouteDefinitionErrors.AlreadyDeprecated();

        RaiseDomainEvent(new RouteDefinitionDeprecatedEvent(RouteDefinitionId, deprecatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RouteDefinitionRegisteredEvent e:
                RouteDefinitionId = e.RouteDefinitionId;
                RouteName = e.RouteName;
                SourceRoute = e.SourceRoute;
                DestinationRoute = e.DestinationRoute;
                TransportHint = e.TransportHint;
                Priority = e.Priority;
                Status = RouteDefinitionStatus.Active;
                break;

            case RouteDefinitionDeactivatedEvent:
                Status = RouteDefinitionStatus.Inactive;
                break;

            case RouteDefinitionDeprecatedEvent:
                Status = RouteDefinitionStatus.Deprecated;
                break;
        }
    }
}
