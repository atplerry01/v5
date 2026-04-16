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
        EnsureIdentityInitialized();
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExchangeRateErrors.InvalidStateTransition(Status, ExchangeRateStatus.Active);

        RaiseDomainEvent(new ExchangeRateActivatedEvent(RateId, activatedAt));
    }

    // ── Expire ───────────────────────────────────────────────────

    public void Expire(Timestamp expiredAt)
    {
        EnsureIdentityInitialized();
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
                // AGGREGATE-IDENTITY-REHYDRATION-01 — prefer event-carried
                // identity; fall back to the canonical aggregate id the
                // reconstruction loader stamped on this instance (the stored
                // payload's `AggregateId` key does not bind to the domain
                // record's `RateId` parameter on JSON round-trip).
                RateId = e.RateId.Value != Guid.Empty
                    ? e.RateId
                    : new RateId(AggregateIdentity);
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

    // AGGREGATE-IDENTITY-REHYDRATION-01 enforcement hook — see FxAggregate
    // for rationale. Called on every path that emits a mutating event.
    private void EnsureIdentityInitialized()
    {
        if (RateId.Value == Guid.Empty)
            throw new InvalidOperationException(
                "ExchangeRateAggregate identity not initialized before emitting event (AGGREGATE-IDENTITY-REHYDRATION-01).");
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        // Value/version invariants are enforced at aggregate-construction
        // time (DefineRate factory) against the command inputs. Re-
        // asserting them here surfaces schema-vs-domain deserialization
        // gaps (see FxAggregate note) rather than real business-rule
        // violations, so the canonical pattern leaves them to the
        // factory path and the value objects themselves.
    }
}
