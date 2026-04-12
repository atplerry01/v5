using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;

public sealed class DiscrepancyAggregate : AggregateRoot
{
    public DiscrepancyId DiscrepancyId { get; private set; }
    public ProcessReference ProcessReference { get; private set; }
    public DiscrepancySource Source { get; private set; }
    public Amount ExpectedValue { get; private set; }
    public Amount ActualValue { get; private set; }
    public Amount Difference { get; private set; }
    public DiscrepancyStatus Status { get; private set; }
    public string Resolution { get; private set; } = string.Empty;
    public Timestamp DetectedAt { get; private set; }

    private DiscrepancyAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DiscrepancyAggregate Detect(
        DiscrepancyId discrepancyId,
        ProcessReference processReference,
        DiscrepancySource source,
        Amount expectedValue,
        Amount actualValue,
        Amount difference,
        Timestamp detectedAt)
    {
        if (processReference == default)
            throw DiscrepancyErrors.MissingProcessReference();

        var aggregate = new DiscrepancyAggregate();
        aggregate.RaiseDomainEvent(new DiscrepancyDetectedEvent(
            discrepancyId, processReference, source, expectedValue, actualValue, difference, detectedAt));
        return aggregate;
    }

    // ── Investigate ──────────────────────────────────────────────

    public void Investigate()
    {
        var specification = new CanInvestigateSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw DiscrepancyErrors.InvalidStateTransition(Status, nameof(Investigate));

        RaiseDomainEvent(new DiscrepancyInvestigatedEvent(DiscrepancyId));
    }

    // ── Resolve ──────────────────────────────────────────────────

    public void Resolve(string resolution)
    {
        if (string.IsNullOrWhiteSpace(resolution))
            throw DiscrepancyErrors.EmptyResolution();

        var specification = new CanResolveSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw DiscrepancyErrors.InvalidStateTransition(Status, nameof(Resolve));

        RaiseDomainEvent(new DiscrepancyResolvedEvent(DiscrepancyId, resolution));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DiscrepancyDetectedEvent e:
                DiscrepancyId = e.DiscrepancyId;
                ProcessReference = e.ProcessReference;
                Source = e.Source;
                ExpectedValue = e.ExpectedValue;
                ActualValue = e.ActualValue;
                Difference = e.Difference;
                Status = DiscrepancyStatus.Open;
                DetectedAt = e.DetectedAt;
                break;

            case DiscrepancyInvestigatedEvent:
                Status = DiscrepancyStatus.Investigating;
                break;

            case DiscrepancyResolvedEvent e:
                Status = DiscrepancyStatus.Resolved;
                Resolution = e.Resolution;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (DiscrepancyId == default)
            throw DiscrepancyErrors.MissingId();

        if (ProcessReference == default)
            throw DiscrepancyErrors.MissingProcessReference();

        if (!Enum.IsDefined(Status))
            throw DiscrepancyErrors.InvalidStatus();
    }
}
