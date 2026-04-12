using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public static class CapitalAccountErrors
{
    public static DomainException InsufficientAvailableBalance(Amount requested, Amount available) =>
        new($"Insufficient available balance. Requested: {requested.Value}, Available: {available.Value}.");

    public static DomainException InvalidAmount() =>
        new("Amount must be greater than zero.");

    public static DomainException InvalidCurrencyCode() =>
        new("Currency code is invalid.");

    public static DomainException CurrencyMismatch(Currency expected, Currency actual) =>
        new($"Currency mismatch. Expected: {expected.Code}, Actual: {actual.Code}.");

    public static DomainException AccountIsFrozen() =>
        new("Operation not permitted. Account is frozen.");

    public static DomainException AccountIsClosed() =>
        new("Operation not permitted. Account is closed.");

    public static DomainException CannotCloseWithOutstandingBalance() =>
        new("Cannot close account with an outstanding total balance.");

    public static DomainException CannotCloseWithReservedFunds() =>
        new("Cannot close account with reserved funds.");

    public static DomainInvariantViolationException NegativeTotalBalance() =>
        new("Total balance cannot be negative.");

    public static DomainInvariantViolationException NegativeAvailableBalance() =>
        new("Available balance cannot be negative.");

    public static DomainInvariantViolationException NegativeReservedBalance() =>
        new("Reserved balance cannot be negative.");

    public static DomainInvariantViolationException BalanceInvariantViolation(
        Amount total, Amount available, Amount reserved) =>
        new($"Balance invariant violated. Total: {total.Value}, Available: {available.Value}, Reserved: {reserved.Value}. " +
            "Available + Reserved must equal Total.");
}
