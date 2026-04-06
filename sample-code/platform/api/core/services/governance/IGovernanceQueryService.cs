using Whycespace.Platform.Api.Core.Contracts.Governance;

namespace Whycespace.Platform.Api.Core.Services.Governance;

/// <summary>
/// Read-only governance query service.
/// All data sourced from CQRS governance projections via ProjectionAdapter.
///
/// MUST NOT:
/// - Call engines or domain services
/// - Access raw policy rules or internal votes
/// - Replay events or reconstruct state
/// - Modify any state
/// </summary>
public interface IGovernanceQueryService
{
    Task<GovernanceDecisionView?> GetDecisionAsync(Guid decisionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GovernanceDecisionView>> GetDecisionsByClusterAsync(string cluster, CancellationToken cancellationToken = default);
    Task<GovernanceTimelineView?> GetTimelineAsync(Guid decisionId, CancellationToken cancellationToken = default);
}
