namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public readonly record struct AssetReference
{
    public Guid Value { get; }

    public AssetReference(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssetReference value must not be empty.", nameof(value));

        Value = value;
    }
}
