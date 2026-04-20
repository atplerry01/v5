namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public static class QuoteErrors
{
    public static QuoteDomainException MissingId()
        => new("QuoteId is required and must not be empty.");

    public static QuoteDomainException MissingQuoteBasisRef()
        => new("Quote must reference a quote-basis.");

    public static QuoteDomainException InvalidStateTransition(QuoteStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static QuoteDomainException IssuanceRequiresValidity()
        => new("Quote issuance requires a validity window.");

    public static QuoteDomainException AlreadyTerminal(QuoteId id, QuoteStatus status)
        => new($"Quote '{id.Value}' is already terminal ({status}) and cannot be acted on.");

    public static QuoteDomainException ExpiryNotReached()
        => new("Quote cannot be marked expired before its validity end.");
}

public sealed class QuoteDomainException : Exception
{
    public QuoteDomainException(string message) : base(message) { }
}
