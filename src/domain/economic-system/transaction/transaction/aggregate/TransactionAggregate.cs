using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// Orchestration envelope for economic actions. A transaction binds one
/// or more action references (expense, revenue, future) under a single
/// strict lifecycle: Initiated → Processing → Committed (terminal) or
/// Initiated → Processing → Failed (terminal). Transaction carries
/// references only — it does NOT embed any action's logic. Downstream
/// ledger and capital domains subscribe to `TransactionCommittedEvent` to
/// perform their postings; neither is imported here.
///
/// Lifecycle enforcement: no direct Initiated → Committed and no direct
/// Initiated → Failed transitions. Processing is the mandatory gate — it
/// marks the window during which external execution (settlement rails,
/// ledger posting attempts) may occur. Committed and Failed are terminal
/// and re-entry is guarded.
///
/// Replay-safe: every state mutation goes through an emitted event and
/// `Apply`; the aggregate is reconstructible from its event history
/// with no timestamp inference.
/// </summary>
public sealed class TransactionAggregate : AggregateRoot
{
    private readonly List<TransactionReference> _references = new();

    public TransactionId TransactionId { get; private set; }
    public string Kind { get; private set; } = string.Empty;
    public TransactionStatus Status { get; private set; }
    public Timestamp InitiatedAt { get; private set; }
    public Timestamp? ProcessingStartedAt { get; private set; }
    public Timestamp? CommittedAt { get; private set; }
    public Timestamp? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public IReadOnlyList<TransactionReference> References => _references.AsReadOnly();

    private TransactionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static TransactionAggregate Initiate(
        TransactionId transactionId,
        string kind,
        IReadOnlyList<TransactionReference> references,
        Timestamp initiatedAt)
    {
        if (string.IsNullOrWhiteSpace(kind))
            throw TransactionErrors.MissingKind();
        if (references is null || references.Count == 0)
            throw TransactionErrors.MissingReferences();

        var aggregate = new TransactionAggregate();
        aggregate.RaiseDomainEvent(new TransactionInitiatedEvent(
            transactionId, kind.Trim(), references, initiatedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void MarkProcessing(Timestamp processingStartedAt)
    {
        GuardNotTerminal();

        if (!new CanProcessSpecification().IsSatisfiedBy(this))
            throw TransactionErrors.TransactionNotInitiated();

        RaiseDomainEvent(new TransactionProcessingStartedEvent(
            TransactionId, processingStartedAt));
    }

    public void Commit(Timestamp committedAt)
    {
        GuardNotTerminal();

        if (!new CanCommitSpecification().IsSatisfiedBy(this))
            throw TransactionErrors.TransactionNotProcessing();

        RaiseDomainEvent(new TransactionCommittedEvent(
            TransactionId, Kind, References, committedAt));
    }

    public void Fail(string reason, Timestamp failedAt)
    {
        GuardNotTerminal();

        if (!new CanFailSpecification().IsSatisfiedBy(this))
            throw TransactionErrors.TransactionNotProcessing();

        RaiseDomainEvent(new TransactionFailedEvent(
            TransactionId, reason ?? string.Empty, failedAt));
    }

    private void GuardNotTerminal()
    {
        if (Status == TransactionStatus.Committed) throw TransactionErrors.TransactionAlreadyCommitted();
        if (Status == TransactionStatus.Failed) throw TransactionErrors.TransactionAlreadyFailed();
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TransactionInitiatedEvent e:
                TransactionId = e.TransactionId;
                Kind = e.Kind;
                _references.Clear();
                foreach (var r in e.References) _references.Add(r);
                Status = TransactionStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;

            case TransactionProcessingStartedEvent e:
                Status = TransactionStatus.Processing;
                ProcessingStartedAt = e.ProcessingStartedAt;
                break;

            case TransactionCommittedEvent e:
                Status = TransactionStatus.Committed;
                CommittedAt = e.CommittedAt;
                break;

            case TransactionFailedEvent e:
                Status = TransactionStatus.Failed;
                FailedAt = e.FailedAt;
                FailureReason = e.Reason;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        // References are bootstrapped inside the Initiated event; post-initiation
        // the list is immutable from the outside and cannot be emptied.
    }
}
