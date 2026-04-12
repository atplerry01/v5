using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public static class PoolErrors
{
    public static DomainException InvalidAmount()
        => new("Amount must be greater than zero.");

    public static DomainException PoolAlreadyExists()
        => new("A capital pool with this identifier already exists.");

    public static DomainException InsufficientPoolCapital(Amount requested, Amount available)
        => new($"Insufficient pool capital: requested {requested.Value} but only {available.Value} available.");

    public static DomainException InvalidCurrencyCode()
        => new("Currency code is invalid or empty.");

    public static DomainException CannotReduceBelowZero()
        => new("Capital reduction would bring the pool balance below zero.");

    public static DomainInvariantViolationException NegativePoolBalance()
        => new("Pool total capital must not be negative.");

    public static DomainInvariantViolationException ArtificialCapitalDetected(Amount poolTotal, Amount accountSum)
        => new($"Artificial capital detected: pool total {poolTotal.Value} exceeds account sum {accountSum.Value}.");
}
