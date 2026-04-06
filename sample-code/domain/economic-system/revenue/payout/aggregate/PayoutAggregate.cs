namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed class PayoutAggregate : AggregateRoot
{
    public PayoutRecipient Recipient { get; private set; } = null!;
    public PayoutAmount Amount { get; private set; } = null!;
    public string CurrencyCode { get; private set; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; private set; }
    public PayoutStatus Status { get; private set; } = PayoutStatus.Scheduled;
    public string? FailureReason { get; private set; }
    public string? HoldReason { get; private set; }

    private readonly List<PayoutItem> _items = new();
    public IReadOnlyList<PayoutItem> Items => _items;

    public PayoutAggregate() { }

    public static PayoutAggregate Schedule(
        Guid id,
        PayoutRecipient recipient,
        PayoutAmount amount,
        string currencyCode,
        DateTimeOffset scheduledAt)
    {
        if (id == Guid.Empty)
            throw new PayoutException("Payout id is required.");

        if (recipient is null)
            throw new PayoutException("Payout recipient is required.");

        if (amount is null)
            throw new PayoutException("Payout amount is required.");

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new PayoutException("Currency code is required.");

        var aggregate = new PayoutAggregate
        {
            Id = id,
            Recipient = recipient,
            Amount = amount,
            CurrencyCode = currencyCode,
            ScheduledAt = scheduledAt,
            Status = PayoutStatus.Scheduled
        };

        aggregate.RaiseDomainEvent(new PayoutScheduledEvent(
            id,
            recipient.RecipientId,
            amount.Value,
            currencyCode));

        return aggregate;
    }

    public void AddItem(PayoutItem item)
    {
        EnsureInvariant(
            item is not null,
            "PAYOUT_ITEM_REQUIRED",
            "Payout item cannot be null.");

        EnsureInvariant(
            !Status.IsTerminal,
            "PAYOUT_NOT_TERMINAL",
            "Cannot add items to a completed or failed payout.");

        _items.Add(item!);
    }

    public void Approve()
    {
        EnsureValidTransition(Status, PayoutStatus.Approved,
            (from, to) => from.CanTransitionTo(to));

        Status = PayoutStatus.Approved;
        RaiseDomainEvent(new PayoutApprovedEvent(Id));
    }

    public void Process()
    {
        EnsureValidTransition(Status, PayoutStatus.Processing,
            (from, to) => from.CanTransitionTo(to));

        EnsureInvariant(
            Status == PayoutStatus.Approved,
            "PAYOUT_MUST_BE_APPROVED",
            "Cannot process a payout that has not been approved.");

        Status = PayoutStatus.Processing;
        RaiseDomainEvent(new PayoutProcessedEvent(Id, Amount.Value));
    }

    public void Complete()
    {
        EnsureValidTransition(Status, PayoutStatus.Completed,
            (from, to) => from.CanTransitionTo(to));

        EnsureInvariant(
            Status == PayoutStatus.Processing,
            "PAYOUT_MUST_BE_PROCESSING",
            "Cannot complete a payout that is not being processed.");

        Status = PayoutStatus.Completed;
        RaiseDomainEvent(new PayoutCompletedEvent(Id));
    }

    public void Fail(string reason)
    {
        EnsureValidTransition(Status, PayoutStatus.Failed,
            (from, to) => from.CanTransitionTo(to));

        EnsureInvariant(
            !string.IsNullOrWhiteSpace(reason),
            "FAILURE_REASON_REQUIRED",
            "A reason must be provided when failing a payout.");

        Status = PayoutStatus.Failed;
        FailureReason = reason;
        RaiseDomainEvent(new PayoutFailedEvent(Id, reason));
    }

    public void Hold(string reason)
    {
        EnsureValidTransition(Status, PayoutStatus.Held,
            (from, to) => from.CanTransitionTo(to));

        EnsureInvariant(
            !string.IsNullOrWhiteSpace(reason),
            "HOLD_REASON_REQUIRED",
            "A reason must be provided when holding a payout.");

        Status = PayoutStatus.Held;
        HoldReason = reason;
        RaiseDomainEvent(new PayoutHeldEvent(Id, reason));
    }

    public void Release()
    {
        EnsureValidTransition(Status, PayoutStatus.Approved,
            (from, to) => from.CanTransitionTo(to));

        EnsureInvariant(
            Status == PayoutStatus.Held,
            "PAYOUT_MUST_BE_HELD",
            "Only held payouts can be released.");

        Status = PayoutStatus.Approved;
        HoldReason = null;
        RaiseDomainEvent(new PayoutApprovedEvent(Id));
    }
}
