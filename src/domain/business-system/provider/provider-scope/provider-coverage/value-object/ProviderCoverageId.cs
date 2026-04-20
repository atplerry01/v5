namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public readonly record struct ProviderCoverageId
{
    public Guid Value { get; }

    public ProviderCoverageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderCoverageId value must not be empty.", nameof(value));

        Value = value;
    }
}
