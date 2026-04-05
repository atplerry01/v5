using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public static class CapitalAccountErrors
{
    public static DomainException InsufficientAvailableBalance(decimal requested, decimal available) =>
        new($"Cannot process {requested:F2}: only {available:F2} available.");

    public static DomainException AccountIsFrozen(AccountId accountId) =>
        new($"Capital account '{accountId.Value}' is frozen. No operations permitted.");

    public static DomainException AccountIsClosed(AccountId accountId) =>
        new($"Capital account '{accountId.Value}' is closed. No operations permitted.");

    public static DomainException CannotCloseWithOutstandingBalance(decimal total) =>
        new($"Cannot close account with outstanding balance of {total:F2}.");

    public static DomainException CannotCloseWithReservedFunds(decimal reserved) =>
        new($"Cannot close account with reserved funds of {reserved:F2}.");

    public static DomainException InvalidAmount(decimal amount) =>
        new($"Amount must be greater than zero. Received: {amount:F2}.");

    public static DomainException InvalidCurrencyCode(string code) =>
        new($"Currency code '{code}' is invalid. Must be a non-empty ISO currency code.");

    public static DomainException CurrencyMismatch(string expected, string actual) =>
        new($"Currency mismatch: account uses '{expected}' but received '{actual}'.");

    public static DomainInvariantViolationException NegativeTotalBalance(decimal total) =>
        new($"Invariant violated: TotalBalance cannot be negative. Current: {total:F2}.");

    public static DomainInvariantViolationException NegativeAvailableBalance(decimal available) =>
        new($"Invariant violated: AvailableBalance cannot be negative. Current: {available:F2}.");

    public static DomainInvariantViolationException NegativeReservedBalance(decimal reserved) =>
        new($"Invariant violated: ReservedBalance cannot be negative. Current: {reserved:F2}.");

    public static DomainInvariantViolationException BalanceInvariantViolation(decimal total, decimal available, decimal reserved) =>
        new($"Invariant violated: Available ({available:F2}) + Reserved ({reserved:F2}) must equal Total ({total:F2}).");
}
