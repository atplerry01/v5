using Whycespace.Platform.Api.Core.Contracts.Graph;

namespace Whycespace.Platform.Api.Core.Services.Graph;

/// <summary>
/// Read-only SPV graph query service.
/// All data sourced from pre-built graph projections via ProjectionAdapter.
///
/// MUST NOT:
/// - Call engines or domain services
/// - Traverse domain aggregates
/// - Compute relationships or flows
/// - Perform recursive queries (N+1)
/// - Modify any state
///
/// The graph projection store pre-computes the full graph.
/// This service only reads and maps — nothing more.
/// </summary>
public interface ISpvGraphQueryService
{
    Task<SpvGraphView?> GetGraphAsync(Guid rootSpvId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpvFlowView>> GetFlowsAsync(Guid spvId, CancellationToken cancellationToken = default);
}
