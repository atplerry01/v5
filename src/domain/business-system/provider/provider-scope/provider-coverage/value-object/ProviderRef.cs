namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public readonly record struct ProviderRef
{
    public Guid Value { get; }

    public ProviderRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderRef value must not be empty.", nameof(value));

        Value = value;
    }
}
