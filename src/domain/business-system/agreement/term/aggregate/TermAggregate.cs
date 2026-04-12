namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public sealed class TermAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TermId Id { get; private set; }
    public TermDuration Duration { get; private set; }
    public TermStatus Status { get; private set; }
    public int Version { get; private set; }

    private TermAggregate() { }

    public static TermAggregate Create(TermId id, TermDuration duration)
    {
        var aggregate = new TermAggregate();
        aggregate.ValidateBeforeChange();

        var durationSpec = new IsValidTermDurationSpecification();
        if (!durationSpec.IsSatisfiedBy(duration))
            throw TermErrors.InvalidDuration();

        var @event = new TermCreatedEvent(id, duration);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateTermSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TermErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new TermActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire()
    {
        ValidateBeforeChange();

        var specification = new CanExpireTermSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TermErrors.InvalidStateTransition(Status, nameof(Expire));

        var @event = new TermExpiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TermCreatedEvent @event)
    {
        Id = @event.TermId;
        Duration = @event.Duration;
        Status = TermStatus.Draft;
        Version++;
    }

    private void Apply(TermActivatedEvent @event)
    {
        Status = TermStatus.Active;
        Version++;
    }

    private void Apply(TermExpiredEvent @event)
    {
        Status = TermStatus.Expired;
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
            throw TermErrors.MissingId();

        if (Duration == default)
            throw TermErrors.InvalidDuration();

        if (!Enum.IsDefined(Status))
            throw TermErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
