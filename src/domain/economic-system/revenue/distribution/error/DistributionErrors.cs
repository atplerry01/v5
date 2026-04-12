using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public static class DistributionErrors
{
    public static DomainException InvalidAmount() =>
        new("Distribution amount must be greater than zero.");

    public static DomainException MissingRevenueReference() =>
        new("Distribution must reference a revenue record.");

    public static DomainException InvalidSharePercentage() =>
        new("Share percentage must be between 0 and 100.");

    public static DomainException InvalidRecipient() =>
        new("Allocation must specify a valid recipient.");

    public static DomainInvariantViolationException AllocationsSumMismatch(Amount totalAllocations, Amount totalRevenue) =>
        new($"Invariant violated: sum of allocations ({totalAllocations.Value:F2}) does not equal total revenue ({totalRevenue.Value:F2}).");

    public static DomainInvariantViolationException NegativeDistributionAmount() =>
        new("Invariant violated: distribution amount cannot be negative.");
}
