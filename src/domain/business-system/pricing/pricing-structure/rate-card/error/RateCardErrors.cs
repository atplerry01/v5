using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public static class RateCardErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("RateCardId is required and must not be empty.");

    public static DomainException MissingPriceBookRef()
        => new DomainInvariantViolationException("RateCard must reference a price-book.");

    public static DomainException InvalidStateTransition(RateCardStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException ArchivedImmutable(RateCardId id)
        => new DomainInvariantViolationException($"RateCard '{id.Value}' is archived and cannot be mutated.");

    public static DomainException RateEntryAlreadyPresent(string code)
        => new DomainInvariantViolationException($"RateCard already contains an entry with code '{code}'.");

    public static DomainException RateEntryNotPresent(string code)
        => new DomainInvariantViolationException($"RateCard does not contain an entry with code '{code}'.");

    public static DomainException ActivationRequiresEntries()
        => new DomainInvariantViolationException("RateCard requires at least one rate entry before activation.");

    public static DomainException ActivationRequiresEffectiveWindow()
        => new DomainInvariantViolationException("RateCard requires an effective window before activation.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("RateCard has already been initialized.");
}
