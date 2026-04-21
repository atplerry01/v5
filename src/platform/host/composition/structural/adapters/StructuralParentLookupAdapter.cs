using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Platform.Host.Composition.Structural.Adapters;

/// <summary>
/// Projection-backed implementation of
/// <see cref="IStructuralParentLookup"/>. Reads the cluster and authority
/// read models and maps the projection status strings to the
/// <see cref="StructuralParentState"/> enum.
///
/// Lives in the host adapter layer (not projections) because the Projections
/// project does not reference the Domain project — this adapter bridges
/// Domain contracts to projection stores and belongs in the composition
/// adapter layer by convention.
/// </summary>
public sealed class StructuralParentLookupAdapter : IStructuralParentLookup
{
    private readonly PostgresProjectionStore<ClusterReadModel> _clusterStore;
    private readonly PostgresProjectionStore<AuthorityReadModel> _authorityStore;

    public StructuralParentLookupAdapter(
        PostgresProjectionStore<ClusterReadModel> clusterStore,
        PostgresProjectionStore<AuthorityReadModel> authorityStore)
    {
        _clusterStore = clusterStore;
        _authorityStore = authorityStore;
    }

    public StructuralParentState GetState(ClusterRef parent)
    {
        var state = _clusterStore.LoadAsync(parent.Value, CancellationToken.None)
            .GetAwaiter().GetResult();
        return state is null ? StructuralParentState.Unknown : MapClusterStatus(state.Status);
    }

    public StructuralParentState GetState(ClusterAuthorityRef parent)
    {
        var state = _authorityStore.LoadAsync(parent.Value, CancellationToken.None)
            .GetAwaiter().GetResult();
        return state is null ? StructuralParentState.Unknown : MapAuthorityStatus(state.Status);
    }

    private static StructuralParentState MapClusterStatus(string status) => status switch
    {
        "Active" => StructuralParentState.Active,
        "Defined" => StructuralParentState.Inactive,
        "Archived" => StructuralParentState.Retired,
        _ => StructuralParentState.Unknown
    };

    private static StructuralParentState MapAuthorityStatus(string status) => status switch
    {
        "Active" => StructuralParentState.Active,
        "Established" => StructuralParentState.Inactive,
        "Suspended" => StructuralParentState.Suspended,
        "Revoked" => StructuralParentState.Retired,
        "Retired" => StructuralParentState.Retired,
        _ => StructuralParentState.Unknown
    };
}
