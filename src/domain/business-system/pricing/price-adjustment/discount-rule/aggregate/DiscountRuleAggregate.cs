using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public sealed class DiscountRuleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public DiscountRuleId Id { get; private set; }
    public DiscountRuleCode Code { get; private set; }
    public DiscountRuleName Name { get; private set; }
    public AdjustmentBasis Basis { get; private set; }
    public DiscountAmount Amount { get; private set; }
    public DiscountRuleStatus Status { get; private set; }
    public int Version { get; private set; }

    private DiscountRuleAggregate() { }

    public static DiscountRuleAggregate Create(
        DiscountRuleId id,
        DiscountRuleCode code,
        DiscountRuleName name,
        AdjustmentBasis basis,
        DiscountAmount amount)
    {
        ValidateBasisAmount(basis, amount);

        var aggregate = new DiscountRuleAggregate();

        var @event = new DiscountRuleCreatedEvent(id, code, name, basis, amount);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(DiscountRuleName name, AdjustmentBasis basis, DiscountAmount amount)
    {
        if (Status == DiscountRuleStatus.Archived)
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Update));

        ValidateBasisAmount(basis, amount);

        var @event = new DiscountRuleUpdatedEvent(Id, name, basis, amount);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new DiscountRuleActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new DiscountRuleDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == DiscountRuleStatus.Archived)
            throw DiscountRuleErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new DiscountRuleArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private static void ValidateBasisAmount(AdjustmentBasis basis, DiscountAmount amount)
    {
        if (basis == AdjustmentBasis.Percentage && (amount.Value < 0m || amount.Value > 100m))
            throw DiscountRuleErrors.PercentageOutOfRange(amount.Value);
    }

    private void Apply(DiscountRuleCreatedEvent @event)
    {
        Id = @event.DiscountRuleId;
        Code = @event.Code;
        Name = @event.Name;
        Basis = @event.Basis;
        Amount = @event.Amount;
        Status = DiscountRuleStatus.Draft;
        Version++;
    }

    private void Apply(DiscountRuleUpdatedEvent @event)
    {
        Name = @event.Name;
        Basis = @event.Basis;
        Amount = @event.Amount;
        Version++;
    }

    private void Apply(DiscountRuleActivatedEvent @event)
    {
        Status = DiscountRuleStatus.Active;
        Version++;
    }

    private void Apply(DiscountRuleDeprecatedEvent @event)
    {
        Status = DiscountRuleStatus.Deprecated;
        Version++;
    }

    private void Apply(DiscountRuleArchivedEvent @event)
    {
        Status = DiscountRuleStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw DiscountRuleErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw DiscountRuleErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Basis))
            throw DiscountRuleErrors.InvalidStateTransition(Status, "validate-basis");
    }
}
