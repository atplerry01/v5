namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed class FareRuleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public FareRuleId Id { get; private set; }
    public TariffRef Tariff { get; private set; }
    public FareRuleCode Code { get; private set; }
    public FareRuleCondition Condition { get; private set; }
    public FareRuleStatus Status { get; private set; }
    public int Version { get; private set; }

    private FareRuleAggregate() { }

    public static FareRuleAggregate Create(
        FareRuleId id,
        TariffRef tariff,
        FareRuleCode code,
        FareRuleCondition condition)
    {
        var aggregate = new FareRuleAggregate();

        var @event = new FareRuleCreatedEvent(id, tariff, code, condition);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FareRuleErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new FareRuleActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FareRuleErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new FareRuleDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == FareRuleStatus.Archived)
            throw FareRuleErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new FareRuleArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(FareRuleCreatedEvent @event)
    {
        Id = @event.FareRuleId;
        Tariff = @event.Tariff;
        Code = @event.Code;
        Condition = @event.Condition;
        Status = FareRuleStatus.Draft;
        Version++;
    }

    private void Apply(FareRuleActivatedEvent @event)
    {
        Status = FareRuleStatus.Active;
        Version++;
    }

    private void Apply(FareRuleDeprecatedEvent @event)
    {
        Status = FareRuleStatus.Deprecated;
        Version++;
    }

    private void Apply(FareRuleArchivedEvent @event)
    {
        Status = FareRuleStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw FareRuleErrors.MissingId();

        if (Tariff == default)
            throw FareRuleErrors.MissingTariffRef();

        if (!Enum.IsDefined(Status))
            throw FareRuleErrors.InvalidStateTransition(Status, "validate");
    }
}
