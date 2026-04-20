namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Markup;

public static class MarkupErrors
{
    public static MarkupDomainException MissingId()
        => new("MarkupId is required and must not be empty.");

    public static MarkupDomainException InvalidStateTransition(MarkupStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static MarkupDomainException PercentageOutOfRange(decimal value)
        => new($"MarkupAmount '{value}' is out of the [0, 100] range required for Percentage basis.");
}

public sealed class MarkupDomainException : Exception
{
    public MarkupDomainException(string message) : base(message) { }
}
