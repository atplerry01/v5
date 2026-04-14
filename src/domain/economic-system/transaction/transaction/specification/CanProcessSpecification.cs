using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// A transaction may be transitioned to Processing only from the Initiated
/// state. This gate ensures MarkProcessing is never called on an already-
/// processing or terminal transaction.
/// </summary>
public sealed class CanProcessSpecification : Specification<TransactionAggregate>
{
    public override bool IsSatisfiedBy(TransactionAggregate txn) =>
        txn.Status == TransactionStatus.Initiated;
}
