using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventStream;

public sealed class EventStreamAggregate : AggregateRoot
{
    public EventStreamId EventStreamId { get; private set; }
    public DomainRoute SourceRoute { get; private set; } = null!;
    public IReadOnlyList<string> IncludedEventTypes { get; private set; } = [];
    public OrderingGuarantee OrderingGuarantee { get; private set; }

    private EventStreamAggregate() { }

    public static EventStreamAggregate Declare(
        EventStreamId id,
        DomainRoute sourceRoute,
        IReadOnlyList<string> includedEventTypes,
        OrderingGuarantee orderingGuarantee,
        Timestamp declaredAt)
    {
        var aggregate = new EventStreamAggregate();
        if (aggregate.Version >= 0)
            throw EventStreamErrors.AlreadyInitialized();

        if (!sourceRoute.IsValid())
            throw EventStreamErrors.SourceRouteMissing();

        if (includedEventTypes is null || includedEventTypes.Count == 0)
            throw EventStreamErrors.EventTypeSetEmpty();

        aggregate.RaiseDomainEvent(new EventStreamDeclaredEvent(
            id, sourceRoute, includedEventTypes, orderingGuarantee, declaredAt));

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventStreamDeclaredEvent e:
                EventStreamId = e.EventStreamId;
                SourceRoute = e.SourceRoute;
                IncludedEventTypes = e.IncludedEventTypes;
                OrderingGuarantee = e.OrderingGuarantee;
                break;
        }
    }
}
