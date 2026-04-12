using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public static class TreasuryErrors
{
    public static DomainException InsufficientTreasuryFunds(Amount requested, Amount available)
        => new($"Insufficient treasury funds. Requested: {requested.Value}, Available: {available.Value}.");

    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainInvariantViolationException NegativeTreasuryBalance()
        => new("Invariant violated: treasury balance cannot be negative.");
}
