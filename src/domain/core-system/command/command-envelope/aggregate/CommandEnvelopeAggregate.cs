namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed class CommandEnvelopeAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CommandEnvelopeId Id { get; private set; }
    public EnvelopeMetadata Metadata { get; private set; }
    public CommandEnvelopeStatus Status { get; private set; }
    public int Version { get; private set; }

    private CommandEnvelopeAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static CommandEnvelopeAggregate Seal(
        CommandEnvelopeId id,
        EnvelopeMetadata metadata)
    {
        var aggregate = new CommandEnvelopeAggregate();

        var @event = new CommandEnvelopeSealedEvent(id, metadata);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Dispatch ─────────────────────────────────────────────────

    public void Dispatch()
    {
        var specification = new CanDispatchSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandEnvelopeErrors.InvalidStateTransition(Status, nameof(Dispatch));

        var @event = new CommandEnvelopeDispatchedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Acknowledge ──────────────────────────────────────────────

    public void Acknowledge()
    {
        var specification = new CanAcknowledgeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CommandEnvelopeErrors.InvalidStateTransition(Status, nameof(Acknowledge));

        var @event = new CommandEnvelopeAcknowledgedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(CommandEnvelopeSealedEvent @event)
    {
        Id = @event.EnvelopeId;
        Metadata = @event.Metadata;
        Status = CommandEnvelopeStatus.Sealed;
        Version++;
    }

    private void Apply(CommandEnvelopeDispatchedEvent @event)
    {
        Status = CommandEnvelopeStatus.Dispatched;
        Version++;
    }

    private void Apply(CommandEnvelopeAcknowledgedEvent @event)
    {
        Status = CommandEnvelopeStatus.Acknowledged;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CommandEnvelopeErrors.MissingId();

        if (Metadata == default)
            throw CommandEnvelopeErrors.MissingMetadata();

        if (!Enum.IsDefined(Status))
            throw CommandEnvelopeErrors.InvalidStateTransition(Status, "validate");
    }
}
