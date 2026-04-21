using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderCoverage;

namespace Whycespace.Projections.Business.Provider.ProviderScope.ProviderCoverage.Reducer;

public static class ProviderCoverageProjectionReducer
{
    public static ProviderCoverageReadModel Apply(ProviderCoverageReadModel state, ProviderCoverageCreatedEventSchema e) =>
        state with
        {
            ProviderCoverageId = e.AggregateId,
            ProviderId = e.ProviderId,
            Status = "Draft",
            Scopes = state.Scopes
        };

    public static ProviderCoverageReadModel Apply(ProviderCoverageReadModel state, CoverageScopeAddedEventSchema e) =>
        state with
        {
            ProviderCoverageId = e.AggregateId,
            Scopes = state.Scopes
                .Concat(new[] { new CoverageScopeReadModel(e.ScopeKind, e.ScopeDescriptor) })
                .ToList()
        };

    public static ProviderCoverageReadModel Apply(ProviderCoverageReadModel state, CoverageScopeRemovedEventSchema e) =>
        state with
        {
            ProviderCoverageId = e.AggregateId,
            Scopes = state.Scopes
                .Where(s => !(s.ScopeKind == e.ScopeKind && s.ScopeDescriptor == e.ScopeDescriptor))
                .ToList()
        };

    public static ProviderCoverageReadModel Apply(ProviderCoverageReadModel state, ProviderCoverageActivatedEventSchema e) =>
        state with
        {
            ProviderCoverageId = e.AggregateId,
            Status = "Active"
        };

    public static ProviderCoverageReadModel Apply(ProviderCoverageReadModel state, ProviderCoverageArchivedEventSchema e) =>
        state with
        {
            ProviderCoverageId = e.AggregateId,
            Status = "Archived"
        };
}
