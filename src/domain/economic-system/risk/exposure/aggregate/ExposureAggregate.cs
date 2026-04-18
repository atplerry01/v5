using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public sealed class ExposureAggregate : AggregateRoot
{
    public ExposureId ExposureId { get; private set; }
    public SourceId SourceId { get; private set; }
    public ExposureType ExposureType { get; private set; }
    public Amount TotalExposure { get; private set; }
    public Currency Currency { get; private set; }
    public ExposureStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private ExposureAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ExposureAggregate Create(
        ExposureId exposureId,
        SourceId sourceId,
        ExposureType exposureType,
        Amount initialExposure,
        Currency currency,
        Timestamp createdAt)
    {
        if (initialExposure.Value <= 0)
            throw ExposureErrors.InvalidExposureAmount();

        var aggregate = new ExposureAggregate();
        aggregate.RaiseDomainEvent(new ExposureCreatedEvent(
            exposureId, sourceId, exposureType, initialExposure, currency, createdAt));
        return aggregate;
    }

    // ── Increase ─────────────────────────────────────────────────

    public void IncreaseExposure(Amount amount)
    {
        var specification = new CanIncreaseSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExposureErrors.AlreadyClosed();

        if (amount.Value <= 0)
            throw ExposureErrors.InvalidExposureAmount();

        var newTotal = new Amount(TotalExposure.Value + amount.Value);
        RaiseDomainEvent(new ExposureIncreasedEvent(ExposureId, amount, newTotal));
    }

    // ── Reduce ───────────────────────────────────────────────────

    public void ReduceExposure(Amount amount)
    {
        var specification = new CanReduceSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExposureErrors.AlreadyClosed();

        if (amount.Value <= 0)
            throw ExposureErrors.InvalidExposureAmount();

        if (amount.Value > TotalExposure.Value)
            throw ExposureErrors.ReductionExceedsTotal();

        var newTotal = new Amount(TotalExposure.Value - amount.Value);
        RaiseDomainEvent(new ExposureReducedEvent(ExposureId, amount, newTotal));
    }

    // ── DetectBreach ─────────────────────────────────────────────

    /// <summary>
    /// Phase 6 T6.5 — emits <see cref="ExposureBreachedEvent"/> when the
    /// current TotalExposure is strictly greater than the supplied
    /// threshold. No state mutation — breach detection is a pure signal
    /// that downstream enforcement integration consumes. Replay-safe:
    /// identical (TotalExposure, threshold) inputs emit identical events.
    /// </summary>
    public void DetectBreach(Amount threshold, Timestamp detectedAt)
    {
        if (Status == ExposureStatus.Closed)
            throw ExposureErrors.AlreadyClosed();

        if (threshold.Value <= 0)
            throw ExposureErrors.InvalidExposureAmount();

        if (TotalExposure.Value <= threshold.Value)
            return;

        RaiseDomainEvent(new ExposureBreachedEvent(
            ExposureId, TotalExposure, threshold, Currency, detectedAt));
    }

    // ── Close ────────────────────────────────────────────────────

    public void CloseExposure()
    {
        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExposureErrors.InvalidStateTransition(Status, nameof(CloseExposure));

        RaiseDomainEvent(new ExposureClosedEvent(ExposureId));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ExposureCreatedEvent e:
                ExposureId = e.ExposureId;
                SourceId = e.SourceId;
                ExposureType = e.ExposureType;
                TotalExposure = e.TotalExposure;
                Currency = e.Currency;
                Status = ExposureStatus.Active;
                CreatedAt = e.CreatedAt;
                break;

            case ExposureIncreasedEvent e:
                TotalExposure = e.NewTotal;
                Status = ExposureStatus.Active;
                break;

            case ExposureReducedEvent e:
                TotalExposure = e.NewTotal;
                Status = ExposureStatus.Reduced;
                break;

            case ExposureClosedEvent:
                TotalExposure = new Amount(0m);
                Status = ExposureStatus.Closed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (TotalExposure.Value < 0)
            throw ExposureErrors.ExposureMustBeNonNegative();
    }
}
