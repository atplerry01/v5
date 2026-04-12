using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed class ExchangeRateAggregate : AggregateRoot
{
    public RateId RateId { get; private set; }
    public Currency BaseCurrency { get; private set; }
    public Currency QuoteCurrency { get; private set; }
    public decimal RateValue { get; private set; }
    public Timestamp EffectiveAt { get; private set; }
    public ExchangeRateStatus Status { get; private set; }
    public new int Version { get; private set; }

    private ExchangeRateAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ExchangeRateAggregate DefineRate(
        RateId rateId,
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal rateValue,
        Timestamp effectiveAt,
        int version)
    {
        if (rateValue <= 0m)
            throw ExchangeRateErrors.InvalidRateValue(rateValue);

        var aggregate = new ExchangeRateAggregate();
        aggregate.RaiseDomainEvent(new ExchangeRateDefinedEvent(
            rateId, baseCurrency, quoteCurrency, rateValue, effectiveAt, version));
        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExchangeRateErrors.InvalidStateTransition(Status, ExchangeRateStatus.Active);

        RaiseDomainEvent(new ExchangeRateActivatedEvent(RateId, activatedAt));
    }

    // ── Expire ───────────────────────────────────────────────────

    public void Expire(Timestamp expiredAt)
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExchangeRateErrors.InvalidStateTransition(Status, ExchangeRateStatus.Expired);

        RaiseDomainEvent(new ExchangeRateExpiredEvent(RateId, expiredAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ExchangeRateDefinedEvent e:
                RateId = e.RateId;
                BaseCurrency = e.BaseCurrency;
                QuoteCurrency = e.QuoteCurrency;
                RateValue = e.RateValue;
                EffectiveAt = e.EffectiveAt;
                Version = e.Version;
                Status = ExchangeRateStatus.Defined;
                break;

            case ExchangeRateActivatedEvent:
                Status = ExchangeRateStatus.Active;
                break;

            case ExchangeRateExpiredEvent:
                Status = ExchangeRateStatus.Expired;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (RateValue <= 0m)
            throw ExchangeRateErrors.RateValueMustBePositive();

        if (Version <= 0)
            throw ExchangeRateErrors.VersionMustBePositive();
    }
}
