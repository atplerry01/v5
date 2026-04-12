using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class TransactionAggregate : AggregateRoot
{
    public TransactionId TransactionId { get; private set; }
    public Guid InstructionId { get; private set; }
    public Guid JournalId { get; private set; }
    public TransactionStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private TransactionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static TransactionAggregate Initiate(
        TransactionId transactionId,
        Guid instructionId,
        Timestamp initiatedAt)
    {
        if (instructionId == Guid.Empty) throw TransactionErrors.MissingInstructionReference();

        var aggregate = new TransactionAggregate();
        aggregate.RaiseDomainEvent(new TransactionInitiatedEvent(
            transactionId, instructionId, initiatedAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Complete(Guid journalId, Timestamp completedAt)
    {
        if (Status == TransactionStatus.Failed) throw TransactionErrors.CannotCompleteFailedTransaction();
        if (Status == TransactionStatus.Completed) throw TransactionErrors.TransactionAlreadyCompleted();
        if (Status != TransactionStatus.Initiated) throw TransactionErrors.TransactionNotInitiated();
        if (journalId == Guid.Empty) throw TransactionErrors.MissingJournalReference();

        RaiseDomainEvent(new TransactionCompletedEvent(TransactionId, journalId, completedAt));
    }

    public void Fail(string reason, Timestamp failedAt)
    {
        if (Status == TransactionStatus.Completed) throw TransactionErrors.CannotFailCompletedTransaction();
        if (Status == TransactionStatus.Failed) throw TransactionErrors.TransactionAlreadyFailed();
        if (Status != TransactionStatus.Initiated) throw TransactionErrors.TransactionNotInitiated();

        RaiseDomainEvent(new TransactionFailedEvent(TransactionId, reason, failedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TransactionInitiatedEvent e:
                TransactionId = e.TransactionId;
                InstructionId = e.InstructionId;
                Status = TransactionStatus.Initiated;
                CreatedAt = e.InitiatedAt;
                break;

            case TransactionCompletedEvent e:
                JournalId = e.JournalId;
                Status = TransactionStatus.Completed;
                break;

            case TransactionFailedEvent:
                Status = TransactionStatus.Failed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Status == TransactionStatus.Completed && JournalId == Guid.Empty)
            throw TransactionErrors.MissingJournalReference();
    }
}
