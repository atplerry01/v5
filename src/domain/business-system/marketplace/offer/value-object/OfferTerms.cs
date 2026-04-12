namespace Whycespace.Domain.BusinessSystem.Marketplace.Offer;

public sealed record OfferTerms
{
    public string Description { get; }

    public OfferTerms(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Offer terms description must not be empty.", nameof(description));
        Description = description;
    }
}
