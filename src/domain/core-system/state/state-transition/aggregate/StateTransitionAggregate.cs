namespace Whycespace.Domain.CoreSystem.State.StateTransition;

public sealed class StateTransitionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public StateTransitionId Id { get; private set; }
    public TransitionRule Rule { get; private set; }
    public TransitionStatus Status { get; private set; }
    public int Version { get; private set; }

    private StateTransitionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static StateTransitionAggregate Define(
        StateTransitionId id,
        TransitionRule rule)
    {
        var aggregate = new StateTransitionAggregate();

        var @event = new StateTransitionDefinedEvent(id, rule);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateTransitionErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new StateTransitionActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Retire ───────────────────────────────────────────────────

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateTransitionErrors.InvalidStateTransition(Status, nameof(Retire));

        var @event = new StateTransitionRetiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(StateTransitionDefinedEvent @event)
    {
        Id = @event.TransitionId;
        Rule = @event.Rule;
        Status = TransitionStatus.Defined;
        Version++;
    }

    private void Apply(StateTransitionActivatedEvent @event)
    {
        Status = TransitionStatus.Active;
        Version++;
    }

    private void Apply(StateTransitionRetiredEvent @event)
    {
        Status = TransitionStatus.Retired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw StateTransitionErrors.MissingId();

        if (Rule == default)
            throw StateTransitionErrors.MissingTransitionRule();

        if (!Enum.IsDefined(Status))
            throw StateTransitionErrors.InvalidStateTransition(Status, "validate");
    }
}
