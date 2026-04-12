using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed class IsWithinLimitSpecification : Specification<LimitAggregate>
{
    private readonly Amount _transactionAmount;

    public IsWithinLimitSpecification(Amount transactionAmount)
    {
        _transactionAmount = transactionAmount;
    }

    public override bool IsSatisfiedBy(LimitAggregate limit) =>
        limit.Status == LimitStatus.Active &&
        limit.CurrentUtilization.Value + _transactionAmount.Value <= limit.Threshold.Value;
}
