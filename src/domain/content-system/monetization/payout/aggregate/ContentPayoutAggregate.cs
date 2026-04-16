using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public sealed class ContentPayoutAggregate : AggregateRoot
{
    private static readonly PayoutSpecification Spec = new();

    public ContentPayoutId PayoutId { get; private set; }
    public string ContentRef { get; private set; } = string.Empty;
    public decimal GrossAmount { get; private set; }
    public string CurrencyCode { get; private set; } = string.Empty;
    public ContentPayoutStatus Status { get; private set; }
    public string? SettlementRef { get; private set; }
    public string? FailureReason { get; private set; }

    private ContentPayoutAggregate() { }

    public static ContentPayoutAggregate Calculate(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ContentPayoutId id, string contentRef, decimal gross, string currency, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(contentRef)) throw PayoutErrors.InvalidContentRef();
        if (gross <= 0m) throw PayoutErrors.InvalidGrossAmount();
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw PayoutErrors.InvalidCurrency();
        var agg = new ContentPayoutAggregate();
        agg.RaiseDomainEvent(new ContentPayoutCalculatedEvent(
            eventId, aggregateId, correlationId, causationId, id, contentRef, gross,
            currency.Trim().ToUpperInvariant(), at));
        return agg;
    }

    public void Approve(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string approverRef, Timestamp at)
    {
        Spec.EnsureApprovable(Status);
        if (string.IsNullOrWhiteSpace(approverRef)) throw PayoutErrors.InvalidBeneficiary();
        RaiseDomainEvent(new ContentPayoutApprovedEvent(eventId, aggregateId, correlationId, causationId, PayoutId, approverRef, at));
    }

    public void Settle(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string settlementRef, Timestamp at)
    {
        Spec.EnsureSettleable(Status);
        if (string.IsNullOrWhiteSpace(settlementRef)) throw PayoutErrors.InvalidBeneficiary();
        RaiseDomainEvent(new ContentPayoutSettledEvent(eventId, aggregateId, correlationId, causationId, PayoutId, settlementRef, at));
    }

    public void Fail(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        if (Status == ContentPayoutStatus.Failed) throw PayoutErrors.AlreadyFailed();
        RaiseDomainEvent(new ContentPayoutFailedEvent(eventId, aggregateId, correlationId, causationId, PayoutId, reason ?? string.Empty, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ContentPayoutCalculatedEvent e:
                PayoutId = e.PayoutId;
                ContentRef = e.ContentRef;
                GrossAmount = e.GrossAmount;
                CurrencyCode = e.CurrencyCode;
                Status = ContentPayoutStatus.Calculated;
                break;
            case ContentPayoutApprovedEvent: Status = ContentPayoutStatus.Approved; break;
            case ContentPayoutSettledEvent e:
                Status = ContentPayoutStatus.Settled;
                SettlementRef = e.SettlementRef;
                break;
            case ContentPayoutFailedEvent e:
                Status = ContentPayoutStatus.Failed;
                FailureReason = e.Reason;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(ContentRef))
            throw PayoutErrors.ContentMissing();
    }
}
