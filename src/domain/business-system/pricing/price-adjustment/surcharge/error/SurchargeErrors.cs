using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public static class SurchargeErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("SurchargeId is required and must not be empty.");

    public static DomainException InvalidStateTransition(SurchargeStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException PercentageOutOfRange(decimal value)
        => new DomainInvariantViolationException($"SurchargeAmount '{value}' is out of the [0, 100] range required for Percentage basis.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Surcharge has already been initialized.");
}
