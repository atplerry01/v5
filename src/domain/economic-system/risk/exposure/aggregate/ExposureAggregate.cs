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
