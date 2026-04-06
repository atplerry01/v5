namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed class ChargeAggregate : AggregateRoot
{
    public Guid TargetEntityId { get; private set; }
    public ChargeAmount Amount { get; private set; } = null!;
    public ChargeType Type { get; private set; } = null!;
    public ChargeStatus Status { get; private set; } = ChargeStatus.Created;
    public string? WaiverReason { get; private set; }
    public string? ReversalReason { get; private set; }

    private readonly List<ChargeLineItem> _lineItems = [];
    public IReadOnlyList<ChargeLineItem> LineItems => _lineItems.AsReadOnly();

    public ChargeAggregate() { }

    public static ChargeAggregate Create(
        Guid id,
        Guid targetEntityId,
        ChargeAmount amount,
        ChargeType chargeType)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(targetEntityId);
        Guard.AgainstNull(amount);
        Guard.AgainstNull(chargeType);

        var aggregate = new ChargeAggregate
        {
            Id = id,
            TargetEntityId = targetEntityId,
            Amount = amount,
            Type = chargeType,
            Status = ChargeStatus.Created
        };

        aggregate.RaiseDomainEvent(new ChargeCreatedEvent(
            id, targetEntityId, amount.Value, chargeType.Code));

        return aggregate;
    }

    public void AddLineItem(ChargeLineItem lineItem)
    {
        Guard.AgainstNull(lineItem);

        EnsureInvariant(
            Status == ChargeStatus.Created,
            "CHARGE_MUST_BE_DRAFT",
            "Line items can only be added to charges in 'created' status.");

        _lineItems.Add(lineItem);
    }

    public void Approve()
    {
        EnsureValidTransition(Status, ChargeStatus.Approved,
            (from, to) => from.CanTransitionTo(to));

        Status = ChargeStatus.Approved;
        RaiseDomainEvent(new ChargeApprovedEvent(Id));
    }

    public void Apply()
    {
        EnsureValidTransition(Status, ChargeStatus.Applied,
            (from, to) => from.CanTransitionTo(to));

        Status = ChargeStatus.Applied;
        RaiseDomainEvent(new ChargeAppliedEvent(Id, TargetEntityId, Amount.Value));
    }

    public void Waive(string reason)
    {
        Guard.AgainstEmpty(reason);

        EnsureValidTransition(Status, ChargeStatus.Waived,
            (from, to) => from.CanTransitionTo(to));

        Status = ChargeStatus.Waived;
        WaiverReason = reason;
        RaiseDomainEvent(new ChargeWaivedEvent(Id, reason));
    }

    public void Reverse(string reason)
    {
        Guard.AgainstEmpty(reason);

        EnsureInvariant(
            ChargeReversibleSpecification.IsSatisfiedBy(this),
            "CHARGE_MUST_BE_REVERSIBLE",
            $"Charge cannot be reversed from status '{Status.Value}'.");

        Status = ChargeStatus.Reversed;
        ReversalReason = reason;
        RaiseDomainEvent(new ChargeReversedEvent(Id, reason));
    }
}
