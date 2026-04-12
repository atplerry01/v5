using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed class FxAggregate : AggregateRoot
{
    public FxId FxId { get; private set; }
    public CurrencyPair CurrencyPair { get; private set; }
    public FxStatus Status { get; private set; }

    private FxAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static FxAggregate Register(
        FxId fxId,
        CurrencyPair currencyPair)
    {
        var aggregate = new FxAggregate();
        aggregate.RaiseDomainEvent(new FxPairRegisteredEvent(fxId, currencyPair));
        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw FxErrors.InvalidStateTransition(Status, FxStatus.Active);

        RaiseDomainEvent(new FxPairActivatedEvent(FxId, activatedAt));
    }

    // ── Deactivate ───────────────────────────────────────────────

    public void Deactivate(Timestamp deactivatedAt)
    {
        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw FxErrors.InvalidStateTransition(Status, FxStatus.Deactivated);

        RaiseDomainEvent(new FxPairDeactivatedEvent(FxId, deactivatedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case FxPairRegisteredEvent e:
                FxId = e.FxId;
                CurrencyPair = e.CurrencyPair;
                Status = FxStatus.Defined;
                break;

            case FxPairActivatedEvent:
                Status = FxStatus.Active;
                break;

            case FxPairDeactivatedEvent:
                Status = FxStatus.Deactivated;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (FxId == default)
            throw FxErrors.MissingId();

        if (CurrencyPair == default)
            throw FxErrors.MissingCurrencyPair();
    }
}
