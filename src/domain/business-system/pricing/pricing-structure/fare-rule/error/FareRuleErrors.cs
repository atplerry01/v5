using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public static class FareRuleErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("FareRuleId is required and must not be empty.");

    public static DomainException MissingTariffRef()
        => new DomainInvariantViolationException("FareRule must reference a tariff.");

    public static DomainException InvalidStateTransition(FareRuleStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("FareRule has already been initialized.");
}
