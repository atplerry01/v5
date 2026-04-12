namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public static class ListingErrors
{
    public static ListingDomainException MissingId()
        => new("ListingId is required and must not be empty.");

    public static ListingDomainException MissingOwnerId()
        => new("ListingOwnerId is required and must not be empty.");

    public static ListingDomainException MissingItemReference()
        => new("ListingItemReference is required and must not be empty.");

    public static ListingDomainException InvalidStateTransition(ListingStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ListingDomainException : Exception
{
    public ListingDomainException(string message) : base(message) { }
}
