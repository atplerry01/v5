namespace Whycespace.Domain.EconomicSystem.Transaction.Transaction;

public sealed class DistinctWalletSpec
{
    public bool IsSatisfiedBy(Guid sourceWalletId, Guid destinationWalletId) =>
        sourceWalletId != destinationWalletId;
}
