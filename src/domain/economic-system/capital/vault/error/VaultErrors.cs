using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public static class VaultErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException InvalidAmount() =>
        new("Amount must be greater than zero.");

    public static DomainException InvalidCurrencyCode() =>
        new("Currency code is invalid. Must be a non-empty ISO currency code.");

    public static DomainException SliceNotFound(SliceId sliceId) =>
        new($"Vault slice '{sliceId.Value}' not found.");

    public static DomainException SliceCapacityExceeded(SliceId sliceId, Amount requested, Amount available) =>
        new($"Cannot allocate {requested.Value:F2} from slice '{sliceId.Value}': only {available.Value:F2} available.");

    public static DomainException InsufficientSliceCapacity(SliceId sliceId, Amount requested, Amount available) =>
        new($"Cannot withdraw {requested.Value:F2} from slice '{sliceId.Value}': only {available.Value:F2} available.");

    public static DomainException InsufficientSliceAllocation(SliceId sliceId, Amount requested, Amount used) =>
        new($"Cannot release {requested.Value:F2} from slice '{sliceId.Value}': only {used.Value:F2} allocated.");

    public static DomainException InvalidSliceState(SliceId sliceId, SliceStatus status) =>
        new($"Slice '{sliceId.Value}' is in state '{status}' and cannot perform this operation.");

    public static DomainException DuplicateSliceId(SliceId sliceId) =>
        new($"A slice with id '{sliceId.Value}' already exists in this vault.");

    public static DomainException CurrencyMismatch(Currency expected, Currency actual) =>
        new($"Currency mismatch: vault requires '{expected.Code}' but received '{actual.Code}'.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException NegativeVaultBalance() =>
        new("Invariant violated: vault total stored cannot be negative.");

    public static DomainInvariantViolationException VaultTotalMismatch(Amount vaultTotal, Amount sliceSum) =>
        new($"Invariant violated: vault total ({vaultTotal.Value:F2}) does not equal sum of slice capacities ({sliceSum.Value:F2}).");

    public static DomainInvariantViolationException SliceCapacityInvariantViolation(
        SliceId sliceId, Amount capacity, Amount used, Amount available) =>
        new($"Invariant violated: slice '{sliceId.Value}' capacity ({capacity.Value:F2}) != used ({used.Value:F2}) + available ({available.Value:F2}).");

    public static DomainInvariantViolationException NegativeSliceAvailable(SliceId sliceId) =>
        new($"Invariant violated: slice '{sliceId.Value}' available amount cannot be negative.");

    public static DomainInvariantViolationException NegativeSliceUsed(SliceId sliceId) =>
        new($"Invariant violated: slice '{sliceId.Value}' used amount cannot be negative.");
}
