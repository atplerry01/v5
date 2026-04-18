using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public static class LimitErrors
{
    public static DomainException LimitExceeded(Amount attempted, Amount threshold) =>
        new($"Transaction amount {attempted.Value:F2} exceeds limit threshold {threshold.Value:F2}.");

    public static DomainException LimitNotActive() =>
        new("Limit must be Active to validate against.");

    public static DomainException InvalidThreshold() =>
        new("Threshold must be greater than zero.");

    public static DomainException MissingAccountReference() =>
        new("Limit must reference an account.");

    public static DomainInvariantViolationException NegativeThreshold() =>
        new("Invariant violated: limit threshold cannot be negative.");

    public static DomainInvariantViolationException NegativeUtilization() =>
        new("Invariant violated: limit utilization cannot be negative.");

    public static DomainException ConcurrencyConflict(int expectedVersion, int actualVersion) =>
        new($"Limit concurrency conflict: expected version {expectedVersion}, actual version {actualVersion}. " +
            "Another check has already been applied to this limit; reload and retry.");
}
