namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public readonly record struct BidId
{
    public Guid Value { get; }

    public BidId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BidId value must not be empty.", nameof(value));

        Value = value;
    }
}
