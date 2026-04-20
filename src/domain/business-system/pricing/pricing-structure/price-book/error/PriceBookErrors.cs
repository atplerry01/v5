namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public static class PriceBookErrors
{
    public static PriceBookDomainException MissingId()
        => new("PriceBookId is required and must not be empty.");

    public static PriceBookDomainException InvalidStateTransition(PriceBookStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static PriceBookDomainException EffectiveWindowRequiredForActivation()
        => new("PriceBook requires an effective window before activation.");
}

public sealed class PriceBookDomainException : Exception
{
    public PriceBookDomainException(string message) : base(message) { }
}
