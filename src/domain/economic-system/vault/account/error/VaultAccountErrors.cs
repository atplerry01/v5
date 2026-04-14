using Whycespace.Domain.EconomicSystem.Vault.Slice;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public static class VaultAccountErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException InvalidAmount() =>
        new("Amount must be greater than zero.");

    public static DomainException CurrencyMismatch(Currency expected, Currency actual) =>
        new($"Currency mismatch: vault account requires '{expected.Code}' but received '{actual.Code}'.");

    public static DomainException AccountIsClosed() =>
        new("Vault account is closed and cannot accept operations.");

    public static DomainException OnlySlice1AcceptsFunding(SliceType attempted) =>
        new($"Funding is restricted to Slice1 (liquidity gateway). Attempted: {attempted}.");

    public static DomainException OnlySlice1ToSlice2Investment(SliceType from, SliceType to) =>
        new($"Investment path is restricted to Slice1 → Slice2. Attempted: {from} → {to}.");

    public static DomainException OnlySlice1Payout(SliceType attempted) =>
        new($"Payout debit/credit is restricted to Slice1. Attempted: {attempted}.");

    public static DomainException InsufficientFreeCapital(Amount requested, Amount available) =>
        new($"Cannot allocate {requested.Value:F2}: only {available.Value:F2} free in Slice1.");

    public static DomainException SliceNotFound(SliceType sliceType) =>
        new($"Slice '{sliceType}' not found on this vault account.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException SliceCountInvariantViolation(int actual) =>
        new($"Invariant violated: vault account must own exactly 4 slices (has {actual}).");

    public static DomainInvariantViolationException DuplicateSliceTypeInvariantViolation(SliceType sliceType) =>
        new($"Invariant violated: slice type '{sliceType}' appears more than once on vault account.");
}
