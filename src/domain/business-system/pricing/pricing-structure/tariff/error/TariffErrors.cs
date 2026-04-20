namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public static class TariffErrors
{
    public static TariffDomainException MissingId()
        => new("TariffId is required and must not be empty.");

    public static TariffDomainException MissingPriceBookRef()
        => new("Tariff must reference a price-book.");

    public static TariffDomainException InvalidStateTransition(TariffStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static TariffDomainException ActivationRequiresEffectiveWindow()
        => new("Tariff requires an effective window before activation.");
}

public sealed class TariffDomainException : Exception
{
    public TariffDomainException(string message) : base(message) { }
}
