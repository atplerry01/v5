namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public readonly record struct ListingItemReference
{
    public Guid Value { get; }

    public ListingItemReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ListingItemReference value must not be empty.", nameof(value));
        Value = value;
    }
}
