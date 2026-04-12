namespace Whycespace.Domain.BusinessSystem.Resource.AssetResource;

public readonly record struct AssetResourceId
{
    public Guid Value { get; }

    public AssetResourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssetResourceId value must not be empty.", nameof(value));

        Value = value;
    }
}
