namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;

public enum CoverageScopeKind
{
    Region,
    Market,
    Domain,
    Custom
}

public readonly record struct CoverageScope
{
    public const int MaxDescriptorLength = 200;

    public CoverageScopeKind Kind { get; }
    public string Descriptor { get; }

    public CoverageScope(CoverageScopeKind kind, string descriptor)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentException("CoverageScopeKind is invalid.", nameof(kind));

        if (string.IsNullOrWhiteSpace(descriptor))
            throw new ArgumentException("CoverageScope descriptor must not be empty.", nameof(descriptor));

        var trimmed = descriptor.Trim();
        if (trimmed.Length > MaxDescriptorLength)
            throw new ArgumentException($"CoverageScope descriptor exceeds {MaxDescriptorLength} characters.", nameof(descriptor));

        Kind = kind;
        Descriptor = trimmed;
    }
}
