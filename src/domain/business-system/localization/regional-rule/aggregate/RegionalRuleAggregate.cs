namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public sealed class RegionalRuleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RegionalRuleId Id { get; private set; }
    public RegionalRuleStatus Status { get; private set; }
    public RuleCode Code { get; private set; }
    public int Version { get; private set; }

    private RegionalRuleAggregate() { }

    public static RegionalRuleAggregate Create(RegionalRuleId id, RuleCode code)
    {
        var aggregate = new RegionalRuleAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RegionalRuleCreatedEvent(id, code);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RegionalRuleErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new RegionalRuleActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RegionalRuleErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new RegionalRuleSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reactivate()
    {
        ValidateBeforeChange();

        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RegionalRuleErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new RegionalRuleReactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RegionalRuleErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new RegionalRuleArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RegionalRuleCreatedEvent @event)
    {
        Id = @event.RegionalRuleId;
        Code = @event.Code;
        Status = RegionalRuleStatus.Draft;
        Version++;
    }

    private void Apply(RegionalRuleActivatedEvent @event)
    {
        Status = RegionalRuleStatus.Active;
        Version++;
    }

    private void Apply(RegionalRuleSuspendedEvent @event)
    {
        Status = RegionalRuleStatus.Suspended;
        Version++;
    }

    private void Apply(RegionalRuleReactivatedEvent @event)
    {
        Status = RegionalRuleStatus.Active;
        Version++;
    }

    private void Apply(RegionalRuleArchivedEvent @event)
    {
        Status = RegionalRuleStatus.Archived;
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
            throw RegionalRuleErrors.MissingId();

        if (Code == default)
            throw RegionalRuleErrors.InvalidRuleCode();

        if (!Enum.IsDefined(Status))
            throw RegionalRuleErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
