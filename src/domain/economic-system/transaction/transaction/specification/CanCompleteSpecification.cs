using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class CanCompleteSpecification : Specification<TransactionAggregate>
{
    public override bool IsSatisfiedBy(TransactionAggregate txn) =>
        txn.Status == TransactionStatus.Initiated;
}
