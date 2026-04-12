using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Instruction;

public sealed class TransactionInstructionAggregate : AggregateRoot
{
    public InstructionId InstructionId { get; private set; }
    public Guid FromAccountId { get; private set; }
    public Guid ToAccountId { get; private set; }
    public Amount Amount { get; private set; }
    public Currency Currency { get; private set; }
    public InstructionType Type { get; private set; }
    public InstructionStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private TransactionInstructionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static TransactionInstructionAggregate CreateInstruction(
        InstructionId instructionId,
        Guid fromAccountId,
        Guid toAccountId,
        Amount amount,
        Currency currency,
        InstructionType type,
        Timestamp createdAt)
    {
        if (amount.Value <= 0m) throw InstructionErrors.InvalidAmount();
        if (fromAccountId == Guid.Empty) throw InstructionErrors.InvalidFromAccount();
        if (toAccountId == Guid.Empty) throw InstructionErrors.InvalidToAccount();
        if (fromAccountId == toAccountId) throw InstructionErrors.AccountsMustDiffer();

        var aggregate = new TransactionInstructionAggregate();
        aggregate.RaiseDomainEvent(new TransactionInstructionCreatedEvent(
            instructionId, fromAccountId, toAccountId, amount, currency, type, createdAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void MarkExecuted(Timestamp executedAt)
    {
        if (Status == InstructionStatus.Cancelled) throw InstructionErrors.CannotExecuteCancelledInstruction();
        if (Status == InstructionStatus.Executed) throw InstructionErrors.InstructionAlreadyExecuted();
        if (Status != InstructionStatus.Pending) throw InstructionErrors.InstructionNotPending();

        RaiseDomainEvent(new TransactionInstructionExecutedEvent(InstructionId, executedAt));
    }

    public void CancelInstruction(string reason, Timestamp cancelledAt)
    {
        if (Status == InstructionStatus.Executed) throw InstructionErrors.CannotCancelExecutedInstruction();
        if (Status == InstructionStatus.Cancelled) throw InstructionErrors.InstructionAlreadyCancelled();
        if (Status != InstructionStatus.Pending) throw InstructionErrors.InstructionNotPending();

        RaiseDomainEvent(new TransactionInstructionCancelledEvent(InstructionId, reason, cancelledAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TransactionInstructionCreatedEvent e:
                InstructionId = e.InstructionId;
                FromAccountId = e.FromAccountId;
                ToAccountId = e.ToAccountId;
                Amount = e.Amount;
                Currency = e.Currency;
                Type = e.Type;
                Status = InstructionStatus.Pending;
                CreatedAt = e.CreatedAt;
                break;

            case TransactionInstructionExecutedEvent:
                Status = InstructionStatus.Executed;
                break;

            case TransactionInstructionCancelledEvent:
                Status = InstructionStatus.Cancelled;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Amount.Value < 0m) throw InstructionErrors.NegativeAmount();
    }
}
