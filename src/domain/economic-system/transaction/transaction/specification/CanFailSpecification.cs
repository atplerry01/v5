using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

/// <summary>
/// A transaction may be failed only from the Processing state. Direct
/// Initiated → Failed is forbidden — a freshly initiated transaction must
/// first transition to Processing (even if only to record the attempt)
/// before it can be recorded as failed. This preserves audit symmetry
/// with the Committed path.
/// </summary>
public sealed class CanFailSpecification : Specification<TransactionAggregate>
{
    public override bool IsSatisfiedBy(TransactionAggregate txn) =>
        txn.Status == TransactionStatus.Processing;
}
