using Whycespace.Domain.BusinessSystem.Shared.Pricing;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed class SurchargeAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SurchargeId Id { get; private set; }
    public SurchargeCode Code { get; private set; }
    public SurchargeName Name { get; private set; }
    public AdjustmentBasis Basis { get; private set; }
    public SurchargeAmount Amount { get; private set; }
    public SurchargeStatus Status { get; private set; }
    public int Version { get; private set; }

    private SurchargeAggregate() { }

    public static SurchargeAggregate Create(
        SurchargeId id,
        SurchargeCode code,
        SurchargeName name,
        AdjustmentBasis basis,
        SurchargeAmount amount)
    {
        ValidateBasisAmount(basis, amount);

        var aggregate = new SurchargeAggregate();

        var @event = new SurchargeCreatedEvent(id, code, name, basis, amount);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(SurchargeName name, AdjustmentBasis basis, SurchargeAmount amount)
    {
        if (Status == SurchargeStatus.Archived)
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Update));

        ValidateBasisAmount(basis, amount);

        var @event = new SurchargeUpdatedEvent(Id, name, basis, amount);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SurchargeActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new SurchargeDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == SurchargeStatus.Archived)
            throw SurchargeErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new SurchargeArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private static void ValidateBasisAmount(AdjustmentBasis basis, SurchargeAmount amount)
    {
        if (basis == AdjustmentBasis.Percentage && (amount.Value < 0m || amount.Value > 100m))
            throw SurchargeErrors.PercentageOutOfRange(amount.Value);
    }

    private void Apply(SurchargeCreatedEvent @event)
    {
        Id = @event.SurchargeId;
        Code = @event.Code;
        Name = @event.Name;
        Basis = @event.Basis;
        Amount = @event.Amount;
        Status = SurchargeStatus.Draft;
        Version++;
    }

    private void Apply(SurchargeUpdatedEvent @event)
    {
        Name = @event.Name;
        Basis = @event.Basis;
        Amount = @event.Amount;
        Version++;
    }

    private void Apply(SurchargeActivatedEvent @event)
    {
        Status = SurchargeStatus.Active;
        Version++;
    }

    private void Apply(SurchargeDeprecatedEvent @event)
    {
        Status = SurchargeStatus.Deprecated;
        Version++;
    }

    private void Apply(SurchargeArchivedEvent @event)
    {
        Status = SurchargeStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SurchargeErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SurchargeErrors.InvalidStateTransition(Status, "validate");

        if (!Enum.IsDefined(Basis))
            throw SurchargeErrors.InvalidStateTransition(Status, "validate-basis");
    }
}
