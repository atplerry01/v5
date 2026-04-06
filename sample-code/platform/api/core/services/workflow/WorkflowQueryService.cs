using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Workflow;

namespace Whycespace.Platform.Api.Core.Services.Workflow;

/// <summary>
/// Projection-backed workflow query service.
/// Delegates all queries to ProjectionAdapter → IProjectionQuerySource.
/// Pure read-only mapping — no business logic, no event replay, no state reconstruction.
/// </summary>
public sealed class WorkflowQueryService : IWorkflowQueryService
{
    private readonly ProjectionAdapter _projections;

    public WorkflowQueryService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<WorkflowView?> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<WorkflowView>(
            "workflow.instance",
            new Dictionary<string, object> { ["workflowId"] = workflowId },
            cancellationToken: cancellationToken);

        return ExtractData<WorkflowView>(response);
    }

    public async Task<IReadOnlyList<WorkflowView>> GetWorkflowsByIdentityAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryListAsync<WorkflowView>(
            "workflow.instance.by-identity",
            new Dictionary<string, object> { ["identityId"] = identityId },
            cancellationToken: cancellationToken);

        return ExtractList<WorkflowView>(response);
    }

    public async Task<WorkflowTimelineView?> GetTimelineAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<WorkflowTimelineView>(
            "workflow.timeline",
            new Dictionary<string, object> { ["workflowId"] = workflowId },
            cancellationToken: cancellationToken);

        return ExtractData<WorkflowTimelineView>(response);
    }

    private static T? ExtractData<T>(Whycespace.Platform.Middleware.ApiResponse response) where T : class
    {
        if (response.StatusCode is < 200 or >= 300)
            return null;

        return response.Data as T;
    }

    private static IReadOnlyList<T> ExtractList<T>(Whycespace.Platform.Middleware.ApiResponse response)
    {
        if (response.StatusCode is < 200 or >= 300)
            return [];

        return response.Data as IReadOnlyList<T> ?? [];
    }
}
