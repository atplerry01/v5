using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed class EventDefinitionAggregate : AggregateRoot
{
    public EventDefinitionId Id { get; private set; }
    public EventSchema Schema { get; private set; }
    public EventDefinitionStatus Status { get; private set; }

    // ── Factory ──────────────────────────────────────────────────

    public static EventDefinitionAggregate Register(
        EventDefinitionId id,
        EventSchema schema)
    {
        var aggregate = new EventDefinitionAggregate();
        if (aggregate.Version >= 0)
            throw EventDefinitionErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new EventDefinitionRegisteredEvent(id, schema));
        return aggregate;
    }

    // ── Publish ──────────────────────────────────────────────────

    public void Publish()
    {
        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventDefinitionErrors.InvalidStateTransition(Status, nameof(Publish));

        RaiseDomainEvent(new EventDefinitionPublishedEvent(Id));
    }

    // ── Deprecate ────────────────────────────────────────────────

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventDefinitionErrors.InvalidStateTransition(Status, nameof(Deprecate));

        RaiseDomainEvent(new EventDefinitionDeprecatedEvent(Id));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EventDefinitionRegisteredEvent e:
                Id = e.DefinitionId;
                Schema = e.Schema;
                Status = EventDefinitionStatus.Draft;
                break;
            case EventDefinitionPublishedEvent:
                Status = EventDefinitionStatus.Published;
                break;
            case EventDefinitionDeprecatedEvent:
                Status = EventDefinitionStatus.Deprecated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw EventDefinitionErrors.MissingId();

        if (Schema == default)
            throw EventDefinitionErrors.MissingSchema();

        if (!Enum.IsDefined(Status))
            throw EventDefinitionErrors.InvalidStateTransition(Status, "validate");
    }
}
