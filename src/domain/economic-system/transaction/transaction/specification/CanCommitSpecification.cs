using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// A transaction may be committed only from the Processing state.
/// Direct Initiated → Committed is forbidden — Processing is the mandatory
/// gate during which external execution is attempted.
/// </summary>
public sealed class CanCommitSpecification : Specification<TransactionAggregate>
{
    public override bool IsSatisfiedBy(TransactionAggregate txn) =>
        txn.Status == TransactionStatus.Processing;
}
