using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed class LimitEnforcementService
{
    public bool EvaluateTransaction(LimitAggregate limit, Amount transactionAmount) =>
        limit.Status == LimitStatus.Active &&
        limit.CurrentUtilization.Value + transactionAmount.Value <= limit.Threshold.Value;
}
