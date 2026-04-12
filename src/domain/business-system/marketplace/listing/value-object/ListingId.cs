namespace Whycespace.Domain.BusinessSystem.Marketplace.Listing;

public readonly record struct ListingId
{
    public Guid Value { get; }

    public ListingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ListingId value must not be empty.", nameof(value));
        Value = value;
    }
}
