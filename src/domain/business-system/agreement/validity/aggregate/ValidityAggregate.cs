namespace Whycespace.Domain.BusinessSystem.Agreement.Validity;

public sealed class ValidityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ValidityId Id { get; private set; }
    public ValidityStatus Status { get; private set; }
    public int Version { get; private set; }

    private ValidityAggregate() { }

    public static ValidityAggregate Create(ValidityId id)
    {
        var aggregate = new ValidityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ValidityCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Invalidate()
    {
        ValidateBeforeChange();

        var specification = new CanInvalidateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValidityErrors.InvalidStateTransition(Status, nameof(Invalidate));

        var @event = new ValidityInvalidatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire()
    {
        ValidateBeforeChange();

        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValidityErrors.InvalidStateTransition(Status, nameof(Expire));

        var @event = new ValidityExpiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ValidityCreatedEvent @event)
    {
        Id = @event.ValidityId;
        Status = ValidityStatus.Valid;
        Version++;
    }

    private void Apply(ValidityInvalidatedEvent @event)
    {
        Status = ValidityStatus.Invalid;
        Version++;
    }

    private void Apply(ValidityExpiredEvent @event)
    {
        Status = ValidityStatus.Expired;
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
            throw ValidityErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ValidityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
