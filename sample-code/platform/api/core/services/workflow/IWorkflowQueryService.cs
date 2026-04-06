using Whycespace.Platform.Api.Core.Contracts.Workflow;

namespace Whycespace.Platform.Api.Core.Services.Workflow;

/// <summary>
/// Read-only workflow query service.
/// All data sourced from CQRS workflow projections via ProjectionAdapter.
///
/// MUST NOT:
/// - Call engines or domain services
/// - Replay events or reconstruct state
/// - Modify any state
/// - Access aggregates or event stores
///
/// Platform surfaces projection data — nothing more.
/// </summary>
public interface IWorkflowQueryService
{
    Task<WorkflowView?> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowView>> GetWorkflowsByIdentityAsync(Guid identityId, CancellationToken cancellationToken = default);
    Task<WorkflowTimelineView?> GetTimelineAsync(Guid workflowId, CancellationToken cancellationToken = default);
}
