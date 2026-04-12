namespace Whycespace.Domain.BusinessSystem.Integration.Partner;

public readonly record struct PartnerId
{
    public Guid Value { get; }

    public PartnerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PartnerId value must not be empty.", nameof(value));

        Value = value;
    }
}
