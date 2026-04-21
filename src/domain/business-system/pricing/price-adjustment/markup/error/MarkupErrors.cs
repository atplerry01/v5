using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public static class MarkupErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("MarkupId is required and must not be empty.");

    public static DomainException InvalidStateTransition(MarkupStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException PercentageOutOfRange(decimal value)
        => new DomainInvariantViolationException($"MarkupAmount '{value}' is out of the [0, 100] range required for Percentage basis.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Markup has already been initialized.");
}
