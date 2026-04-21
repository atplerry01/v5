using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public sealed class EventStreamAggregate : AggregateRoot
{
    public EventStreamId Id { get; private set; }
    public StreamDescriptor Descriptor { get; private set; }
    public EventStreamStatus Status { get; private set; }

    public static EventStreamAggregate Open(EventStreamId id, StreamDescriptor descriptor)
    {
        var aggregate = new EventStreamAggregate();
        if (aggregate.Version >= 0)
            throw EventStreamErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EventStreamOpenedEvent(id, descriptor));
        return aggregate;
    }

    public void Seal()
    {
        var specification = new CanSealSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventStreamErrors.InvalidStateTransition(Status, nameof(Seal));

        RaiseDomainEvent(new EventStreamSealedEvent(Id));
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventStreamErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new EventStreamArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventStreamOpenedEvent e:
                Id = e.EventStreamId;
                Descriptor = e.Descriptor;
                Status = EventStreamStatus.Open;
                break;
            case EventStreamSealedEvent:
                Status = EventStreamStatus.Sealed;
                break;
            case EventStreamArchivedEvent:
                Status = EventStreamStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw EventStreamErrors.MissingId();

        if (Descriptor == default)
            throw EventStreamErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw EventStreamErrors.InvalidStateTransition(Status, "validate");
    }
}
