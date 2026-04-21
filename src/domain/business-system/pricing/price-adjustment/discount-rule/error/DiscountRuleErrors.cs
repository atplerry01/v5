using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public static class DiscountRuleErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("DiscountRuleId is required and must not be empty.");

    public static DomainException InvalidStateTransition(DiscountRuleStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException PercentageOutOfRange(decimal value)
        => new DomainInvariantViolationException($"DiscountAmount '{value}' is out of the [0, 100] range required for Percentage basis.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("DiscountRule has already been initialized.");
}
