namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed record OfferCreatedEvent(
    OfferId OfferId,
    OfferListingReference ListingReference,
    string TermsDescription);
