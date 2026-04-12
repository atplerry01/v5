namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public sealed record ListingCreatedEvent(
    ListingId ListingId,
    ListingOwnerId OwnerId,
    ListingItemReference ItemReference,
    string ListingTitle);
