namespace Whycespace.Domain.BusinessSystem.Integration.Schema;

public sealed class SchemaAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SchemaId Id { get; private set; }
    public SchemaDefinitionId DefinitionId { get; private set; }
    public SchemaStatus Status { get; private set; }
    public int Version { get; private set; }

    private SchemaAggregate() { }

    public static SchemaAggregate Create(SchemaId id, SchemaDefinitionId definitionId)
    {
        var aggregate = new SchemaAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SchemaCreatedEvent(id, definitionId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Publish()
    {
        ValidateBeforeChange();

        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SchemaErrors.InvalidStateTransition(Status, nameof(Publish));

        var @event = new SchemaPublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkFinalized()
    {
        ValidateBeforeChange();

        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SchemaErrors.InvalidStateTransition(Status, nameof(MarkFinalized));

        var @event = new SchemaFinalizedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SchemaCreatedEvent @event)
    {
        Id = @event.SchemaId;
        DefinitionId = @event.DefinitionId;
        Status = SchemaStatus.Draft;
        Version++;
    }

    private void Apply(SchemaPublishedEvent @event)
    {
        Status = SchemaStatus.Published;
        Version++;
    }

    private void Apply(SchemaFinalizedEvent @event)
    {
        Status = SchemaStatus.Finalized;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SchemaErrors.MissingId();

        if (DefinitionId == default)
            throw SchemaErrors.MissingDefinitionId();

        if (!Enum.IsDefined(Status))
            throw SchemaErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
