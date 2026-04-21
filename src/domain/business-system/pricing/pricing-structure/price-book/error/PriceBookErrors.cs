using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public static class PriceBookErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("PriceBookId is required and must not be empty.");

    public static DomainException InvalidStateTransition(PriceBookStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException EffectiveWindowRequiredForActivation()
        => new DomainInvariantViolationException("PriceBook requires an effective window before activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("PriceBook has already been initialized.");
}
