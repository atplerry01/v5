using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Expense;

/// <summary>
/// Expense aggregate. Tracks an expense's lifecycle from creation through
/// recording or cancellation. Emits events only — the runtime persists
/// and anchors. Amount conservation is the responsibility of downstream
/// ledger/capital integration; this aggregate enforces only expense-local
/// invariants.
/// Lifecycle: Created -> Recorded, Created -> Cancelled.
/// </summary>
public sealed class ExpenseAggregate : AggregateRoot
{
    public ExpenseId ExpenseId { get; private set; }
    public decimal Amount { get; private set; }
    public ExpenseMetadata Metadata { get; private set; } = ExpenseMetadata.Of("USD");
    public ExpenseCategory Category { get; private set; }
    public ExpenseSourceReference SourceReference { get; private set; }
    public ExpenseStatus Status { get; private set; }

    private ExpenseAggregate() { }

    public static ExpenseAggregate Create(
        ExpenseId expenseId,
        decimal amount,
        ExpenseMetadata metadata,
        ExpenseCategory category,
        ExpenseSourceReference sourceReference)
    {
        if (amount <= 0m)
            throw ExpenseErrors.InvalidAmount();
        if (metadata is null)
            throw new ArgumentNullException(nameof(metadata));
        if (string.IsNullOrWhiteSpace(sourceReference.Value))
            throw ExpenseErrors.MissingSourceReference();

        var aggregate = new ExpenseAggregate();

        aggregate.RaiseDomainEvent(new ExpenseCreatedEvent(
            expenseId.Value.ToString(),
            amount,
            metadata.Currency,
            category.Value,
            sourceReference.Value));

        return aggregate;
    }

    public void Record()
    {
        var transition = (From: Status, To: ExpenseStatus.Recorded);
        if (!new ExpenseLifecycleSpecification().IsSatisfiedBy(transition))
        {
            if (Status == ExpenseStatus.Recorded) throw ExpenseErrors.AlreadyRecorded();
            throw ExpenseErrors.InvalidStateTransition(Status, ExpenseStatus.Recorded);
        }

        RaiseDomainEvent(new ExpenseRecordedEvent(
            ExpenseId.Value.ToString(),
            Amount,
            Metadata.Currency));
    }

    public void Cancel(string reason)
    {
        var transition = (From: Status, To: ExpenseStatus.Cancelled);
        if (!new ExpenseLifecycleSpecification().IsSatisfiedBy(transition))
            throw ExpenseErrors.InvalidStateTransition(Status, ExpenseStatus.Cancelled);

        RaiseDomainEvent(new ExpenseCancelledEvent(
            ExpenseId.Value.ToString(),
            reason ?? string.Empty));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ExpenseCreatedEvent e:
                ExpenseId = ExpenseId.From(Guid.Parse(e.ExpenseId));
                Amount = e.Amount;
                Metadata = ExpenseMetadata.Of(e.Currency);
                Category = ExpenseCategory.From(e.Category);
                SourceReference = ExpenseSourceReference.From(e.SourceReference);
                Status = ExpenseStatus.Created;
                break;

            case ExpenseRecordedEvent:
                Status = ExpenseStatus.Recorded;
                break;

            case ExpenseCancelledEvent:
                Status = ExpenseStatus.Cancelled;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Amount < 0m)
            throw ExpenseErrors.NegativeAmount();
    }
}
