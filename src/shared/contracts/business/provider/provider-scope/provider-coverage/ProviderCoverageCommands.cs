using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;

public sealed record CreateProviderCoverageCommand(
    Guid ProviderCoverageId,
    Guid ProviderId) : IHasAggregateId
{
    public Guid AggregateId => ProviderCoverageId;
}

public sealed record AddCoverageScopeCommand(
    Guid ProviderCoverageId,
    string ScopeKind,
    string ScopeDescriptor) : IHasAggregateId
{
    public Guid AggregateId => ProviderCoverageId;
}

public sealed record RemoveCoverageScopeCommand(
    Guid ProviderCoverageId,
    string ScopeKind,
    string ScopeDescriptor) : IHasAggregateId
{
    public Guid AggregateId => ProviderCoverageId;
}

public sealed record ActivateProviderCoverageCommand(Guid ProviderCoverageId) : IHasAggregateId
{
    public Guid AggregateId => ProviderCoverageId;
}

public sealed record ArchiveProviderCoverageCommand(Guid ProviderCoverageId) : IHasAggregateId
{
    public Guid AggregateId => ProviderCoverageId;
}
