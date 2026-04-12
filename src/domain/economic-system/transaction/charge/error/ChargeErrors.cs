using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public static class ChargeErrors
{
    public static DomainException ChargeAlreadyApplied() =>
        new("Charge has already been applied.");

    public static DomainException ChargeNotCalculated() =>
        new("Charge must be in Calculated status to apply.");

    public static DomainException InvalidBaseAmount() =>
        new("Base amount must be greater than zero.");

    public static DomainException InvalidChargeAmount() =>
        new("Charge amount must be non-negative.");

    public static DomainException MissingTransactionReference() =>
        new("Charge must reference a transaction.");

    public static DomainInvariantViolationException NegativeChargeAmount() =>
        new("Invariant violated: charge amount cannot be negative.");
}
