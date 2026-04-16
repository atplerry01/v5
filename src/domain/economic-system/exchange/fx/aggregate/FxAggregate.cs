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
        EnsureIdentityInitialized();
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw FxErrors.InvalidStateTransition(Status, FxStatus.Active);

        RaiseDomainEvent(new FxPairActivatedEvent(FxId, activatedAt));
    }

    // ── Deactivate ───────────────────────────────────────────────

    public void Deactivate(Timestamp deactivatedAt)
    {
        EnsureIdentityInitialized();
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
                // AGGREGATE-IDENTITY-REHYDRATION-01: the stored payload is
                // the schema-mapped form `{"AggregateId":"<guid>",…}` which
                // does not bind to the domain record's typed `FxId`
                // parameter on JSON replay (name mismatch → default(FxId)
                // with Value=Guid.Empty). Prefer the event-carried id when
                // present, otherwise fall back to the canonical aggregate
                // identity stamped by the reconstruction loader so the
                // aggregate's FxId field is always populated post-replay.
                FxId = e.FxId.Value != Guid.Empty
                    ? e.FxId
                    : new FxId(AggregateIdentity);
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

    // AGGREGATE-IDENTITY-REHYDRATION-01 enforcement hook: every mutating
    // method MUST call this before RaiseDomainEvent so a command that lands
    // on a hollow/un-rehydrated aggregate fails fast instead of emitting a
    // zero-id event that then poisons the events row + Kafka partition key.
    private void EnsureIdentityInitialized()
    {
        if (FxId.Value == Guid.Empty)
            throw new InvalidOperationException(
                "FxAggregate identity not initialized before emitting event (AGGREGATE-IDENTITY-REHYDRATION-01).");
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        // FxId and CurrencyPair invariants are enforced at aggregate-
        // construction time (Register factory) and by the value objects
        // themselves. Canonical aggregates (capital/account) do not
        // re-check identity fields in EnsureInvariants because event-
        // replay deserialization populates them from the envelope's
        // aggregate_id, not from the payload's value-object-shaped
        // fields. Enforcing them here would surface a cross-cutting
        // deserialization gap as a domain-layer failure.
    }
}
