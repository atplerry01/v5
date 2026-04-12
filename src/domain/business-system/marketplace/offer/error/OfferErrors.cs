namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public static class OfferErrors
{
    public static OfferDomainException MissingId()
        => new("OfferId is required and must not be empty.");

    public static OfferDomainException MissingListingReference()
        => new("OfferListingReference is required and must not be empty.");

    public static OfferDomainException MissingTerms()
        => new("OfferTerms are required and must not be null.");

    public static OfferDomainException InvalidStateTransition(OfferStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class OfferDomainException : Exception
{
    public OfferDomainException(string message) : base(message) { }
}
