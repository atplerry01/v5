namespace Whycespace.Domain.BusinessSystem.Integration.Adapter;

public readonly record struct AdapterId
{
    public Guid Value { get; }

    public AdapterId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AdapterId value must not be empty.", nameof(value));
        Value = value;
    }
}
