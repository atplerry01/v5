namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public readonly record struct ListingOwnerId
{
    public Guid Value { get; }

    public ListingOwnerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ListingOwnerId value must not be empty.", nameof(value));
        Value = value;
    }
}
