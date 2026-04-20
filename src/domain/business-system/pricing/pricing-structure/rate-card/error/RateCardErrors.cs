namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.RateCard;

public static class RateCardErrors
{
    public static RateCardDomainException MissingId()
        => new("RateCardId is required and must not be empty.");

    public static RateCardDomainException MissingPriceBookRef()
        => new("RateCard must reference a price-book.");

    public static RateCardDomainException InvalidStateTransition(RateCardStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RateCardDomainException ArchivedImmutable(RateCardId id)
        => new($"RateCard '{id.Value}' is archived and cannot be mutated.");

    public static RateCardDomainException RateEntryAlreadyPresent(string code)
        => new($"RateCard already contains an entry with code '{code}'.");

    public static RateCardDomainException RateEntryNotPresent(string code)
        => new($"RateCard does not contain an entry with code '{code}'.");

    public static RateCardDomainException ActivationRequiresEntries()
        => new("RateCard requires at least one rate entry before activation.");

    public static RateCardDomainException ActivationRequiresEffectiveWindow()
        => new("RateCard requires an effective window before activation.");
}

public sealed class RateCardDomainException : Exception
{
    public RateCardDomainException(string message) : base(message) { }
}
