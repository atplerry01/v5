namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class RuleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RuleId Id { get; private set; }
    public RuleDefinition Definition { get; private set; }
    public RuleStatus Status { get; private set; }
    public int Version { get; private set; }

    private RuleAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RuleAggregate Draft(
        RuleId id,
        RuleDefinition definition)
    {
        var aggregate = new RuleAggregate();

        var @event = new RuleDraftedEvent(id, definition);
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
            throw RuleErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new RuleActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Retire ───────────────────────────────────────────────────

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RuleErrors.InvalidStateTransition(Status, nameof(Retire));

        var @event = new RuleRetiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(RuleDraftedEvent @event)
    {
        Id = @event.RuleId;
        Definition = @event.Definition;
        Status = RuleStatus.Draft;
        Version++;
    }

    private void Apply(RuleActivatedEvent @event)
    {
        Status = RuleStatus.Active;
        Version++;
    }

    private void Apply(RuleRetiredEvent @event)
    {
        Status = RuleStatus.Retired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw RuleErrors.MissingId();

        if (Definition == default)
            throw RuleErrors.MissingDefinition();

        if (!Enum.IsDefined(Status))
            throw RuleErrors.InvalidStateTransition(Status, "validate");
    }
}
