using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Reconciliation.Process;

public sealed class ProcessAggregate : AggregateRoot
{
    public ProcessId ProcessId { get; private set; }
    public SourceReference LedgerReference { get; private set; }
    public SourceReference ObservedReference { get; private set; }
    public ReconciliationStatus Status { get; private set; }
    public Timestamp TriggeredAt { get; private set; }

    private ProcessAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ProcessAggregate Trigger(
        ProcessId processId,
        SourceReference ledgerReference,
        SourceReference observedReference,
        Timestamp triggeredAt)
    {
        if (ledgerReference == default)
            throw ProcessErrors.MissingLedgerReference();

        if (observedReference == default)
            throw ProcessErrors.MissingObservedReference();

        var aggregate = new ProcessAggregate();
        aggregate.RaiseDomainEvent(new ReconciliationTriggeredEvent(
            processId, ledgerReference, observedReference, triggeredAt));
        return aggregate;
    }

    // ── MarkMatched ──────────────────────────────────────────────

    public void MarkMatched()
    {
        var specification = new CanMatchSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ProcessErrors.InvalidStateTransition(Status, nameof(MarkMatched));

        RaiseDomainEvent(new ReconciliationMatchedEvent(ProcessId));
    }

    // ── MarkMismatched ───────────────────────────────────────────

    public void MarkMismatched()
    {
        var specification = new CanMatchSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ProcessErrors.InvalidStateTransition(Status, nameof(MarkMismatched));

        RaiseDomainEvent(new ReconciliationMismatchedEvent(ProcessId));
    }

    // ── Resolve ──────────────────────────────────────────────────

    public void Resolve()
    {
        var specification = new CanResolveSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ProcessErrors.InvalidStateTransition(Status, nameof(Resolve));

        RaiseDomainEvent(new ReconciliationResolvedEvent(ProcessId));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReconciliationTriggeredEvent e:
                ProcessId = e.ProcessId;
                LedgerReference = e.LedgerReference;
                ObservedReference = e.ObservedReference;
                Status = ReconciliationStatus.Pending;
                TriggeredAt = e.TriggeredAt;
                break;

            case ReconciliationMatchedEvent:
                Status = ReconciliationStatus.Matched;
                break;

            case ReconciliationMismatchedEvent:
                Status = ReconciliationStatus.Mismatched;
                break;

            case ReconciliationResolvedEvent:
                Status = ReconciliationStatus.Resolved;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (ProcessId == default)
            throw ProcessErrors.MissingId();

        if (LedgerReference == default)
            throw ProcessErrors.MissingLedgerReference();

        if (ObservedReference == default)
            throw ProcessErrors.MissingObservedReference();

        if (!Enum.IsDefined(Status))
            throw ProcessErrors.InvalidStatus();
    }
}
