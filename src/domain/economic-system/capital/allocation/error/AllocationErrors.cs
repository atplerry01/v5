using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public static class AllocationErrors
{
    public static DomainException AllocationAlreadyCompleted() =>
        new("Allocation has already been completed.");

    public static DomainException AllocationAlreadyReleased() =>
        new("Allocation has already been released.");

    public static DomainException InvalidAmount() =>
        new("Amount must be greater than zero.");

    public static DomainException InvalidCurrencyCode() =>
        new("Currency code is invalid.");

    public static DomainException CannotCompleteReleasedAllocation() =>
        new("Cannot complete an allocation that has been released.");

    public static DomainException CannotReleaseCompletedAllocation() =>
        new("Cannot release an allocation that has been completed.");

    public static DomainInvariantViolationException NegativeAmount() =>
        new("Allocation amount cannot be negative.");
}
