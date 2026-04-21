using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public static class TariffErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("TariffId is required and must not be empty.");

    public static DomainException MissingPriceBookRef()
        => new DomainInvariantViolationException("Tariff must reference a price-book.");

    public static DomainException InvalidStateTransition(TariffStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ActivationRequiresEffectiveWindow()
        => new DomainInvariantViolationException("Tariff requires an effective window before activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Tariff has already been initialized.");
}
