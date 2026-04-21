using Whycespace.Domain.SharedKernel.Primitives.Kernel;

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
        Guard.Against(!Enum.IsDefined(kind), "CoverageScopeKind is invalid.");
        Guard.Against(string.IsNullOrWhiteSpace(descriptor), "CoverageScope descriptor must not be empty.");

        var trimmed = descriptor.Trim();
        Guard.Against(trimmed.Length > MaxDescriptorLength, $"CoverageScope descriptor exceeds {MaxDescriptorLength} characters.");

        Kind = kind;
        Descriptor = trimmed;
    }
}
