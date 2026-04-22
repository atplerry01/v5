using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;

public sealed class RouteDescriptorAggregate : AggregateRoot
{
    public RouteDescriptorId RouteDescriptorId { get; private set; }
    public DomainRoute Source { get; private set; } = null!;
    public DomainRoute Destination { get; private set; } = null!;
    public string TransportHint { get; private set; } = string.Empty;
    public int Priority { get; private set; }

    private RouteDescriptorAggregate() { }

    public static RouteDescriptorAggregate Register(
        RouteDescriptorId id,
        DomainRoute source,
        DomainRoute destination,
        string transportHint,
        int priority,
        Timestamp registeredAt)
    {
        var aggregate = new RouteDescriptorAggregate();
        if (aggregate.Version >= 0)
            throw RouteDescriptorErrors.AlreadyInitialized();

        if (!source.IsValid())
            throw RouteDescriptorErrors.InvalidSourceRoute();

        if (!destination.IsValid())
            throw RouteDescriptorErrors.InvalidDestinationRoute();

        if (source == destination)
            throw RouteDescriptorErrors.SelfRoutingForbidden();

        if (string.IsNullOrWhiteSpace(transportHint))
            throw RouteDescriptorErrors.TransportHintMissing();

        aggregate.RaiseDomainEvent(new RouteDescriptorRegisteredEvent(
            id, source, destination, transportHint, priority, registeredAt));

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RouteDescriptorRegisteredEvent e:
                RouteDescriptorId = e.RouteDescriptorId;
                Source = e.Source;
                Destination = e.Destination;
                TransportHint = e.TransportHint;
                Priority = e.Priority;
                break;
        }
    }
}
