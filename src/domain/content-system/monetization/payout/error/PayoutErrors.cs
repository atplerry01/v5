using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Payout;

public static class PayoutErrors
{
    public static DomainException InvalidBeneficiary() => new("Payout beneficiary reference must be non-empty.");
    public static DomainException InvalidShareBasis() => new("Payout share basis must be in (0, 1].");
    public static DomainException InvalidGrossAmount() => new("Gross amount must be positive.");
    public static DomainException InvalidCurrency() => new("Currency must be a 3-letter ISO code.");
    public static DomainException InvalidContentRef() => new("Content reference must be non-empty.");
    public static DomainException NotApproved() => new("Payout is not approved.");
    public static DomainException NotCalculated() => new("Payout is not calculated.");
    public static DomainException AlreadyApproved() => new("Payout is already approved.");
    public static DomainException AlreadySettled() => new("Payout is already settled.");
    public static DomainException AlreadyFailed() => new("Payout has already failed.");
    public static DomainException SharesNotBalanced() => new("Payout shares must sum to exactly 1.");
    public static DomainInvariantViolationException ContentMissing() =>
        new("Invariant violated: payout must reference content.");
}
