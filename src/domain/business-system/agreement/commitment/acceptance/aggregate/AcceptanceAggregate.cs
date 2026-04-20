namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public sealed class AcceptanceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AcceptanceId Id { get; private set; }
    public AcceptanceStatus Status { get; private set; }
    public int Version { get; private set; }

    private AcceptanceAggregate() { }

    public static AcceptanceAggregate Create(AcceptanceId id)
    {
        var aggregate = new AcceptanceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AcceptanceCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Accept()
    {
        ValidateBeforeChange();

        var specification = new CanAcceptSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AcceptanceErrors.InvalidStateTransition(Status, nameof(Accept));

        var @event = new AcceptanceAcceptedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reject()
    {
        ValidateBeforeChange();

        var specification = new CanRejectSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AcceptanceErrors.InvalidStateTransition(Status, nameof(Reject));

        var @event = new AcceptanceRejectedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AcceptanceCreatedEvent @event)
    {
        Id = @event.AcceptanceId;
        Status = AcceptanceStatus.Pending;
        Version++;
    }

    private void Apply(AcceptanceAcceptedEvent @event)
    {
        Status = AcceptanceStatus.Accepted;
        Version++;
    }

    private void Apply(AcceptanceRejectedEvent @event)
    {
        Status = AcceptanceStatus.Rejected;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AcceptanceErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw AcceptanceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
