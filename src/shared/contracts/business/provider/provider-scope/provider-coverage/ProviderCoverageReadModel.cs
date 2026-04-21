namespace Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;

public sealed record ProviderCoverageReadModel
{
    public Guid ProviderCoverageId { get; init; }
    public Guid ProviderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<CoverageScopeReadModel> Scopes { get; init; } = Array.Empty<CoverageScopeReadModel>();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}

public sealed record CoverageScopeReadModel(string ScopeKind, string ScopeDescriptor);
