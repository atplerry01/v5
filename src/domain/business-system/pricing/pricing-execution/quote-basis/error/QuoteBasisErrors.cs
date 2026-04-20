namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public static class QuoteBasisErrors
{
    public static QuoteBasisDomainException MissingId()
        => new("QuoteBasisId is required and must not be empty.");

    public static QuoteBasisDomainException MissingPriceBookRef()
        => new("QuoteBasis must reference a price-book.");

    public static QuoteBasisDomainException InvalidStateTransition(QuoteBasisStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static QuoteBasisDomainException FinalizedImmutable(QuoteBasisId id)
        => new($"QuoteBasis '{id.Value}' is finalized and cannot be edited.");
}

public sealed class QuoteBasisDomainException : Exception
{
    public QuoteBasisDomainException(string message) : base(message) { }
}
