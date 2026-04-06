using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Systems.Midstream.Wss.Router;

/// <summary>
/// WSS workflow router — resolves workflow definition and dispatches intent to runtime.
/// Systems layer: MUST NOT call engines or execute logic.
/// Implements IWorkflowRouter (shared contract) for downstream callers.
/// </summary>
public sealed class WorkflowRouter : IWorkflowRouter
{
    private readonly ContextResolver _contextResolver;
    private readonly WorkflowResolver _workflowResolver;
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IIdGenerator _idGenerator;

    public WorkflowRouter(
        ContextResolver contextResolver,
        WorkflowResolver workflowResolver,
        ISystemIntentDispatcher intentDispatcher,
        IIdGenerator idGenerator)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _workflowResolver = workflowResolver ?? throw new ArgumentNullException(nameof(workflowResolver));
        _intentDispatcher = intentDispatcher ?? throw new ArgumentNullException(nameof(intentDispatcher));
        _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
    }

    /// <summary>
    /// Resolves workflow definition and dispatches intent to runtime boundary.
    /// Flow: Downstream -> WSS (here) -> Intent -> Runtime
    /// </summary>
    public async Task<IntentResult> RouteAsync(
        WorkflowDispatchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Resolve context
        var context = _contextResolver.Resolve(
            request.Cluster,
            request.Subcluster,
            request.Domain,
            request.Context);

        // Resolve workflow definition (validates it exists)
        if (_workflowResolver.HasWorkflow(request.WorkflowId))
        {
            _workflowResolver.Resolve(request.WorkflowId);
        }

        // Dispatch intent to runtime boundary
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = _idGenerator.DeterministicGuid("WorkflowRouter", request.CommandType, request.CorrelationId),
            CommandType = request.CommandType,
            Payload = request.Payload,
            CorrelationId = request.CorrelationId,
            Timestamp = request.Timestamp,
            AggregateId = request.AggregateId,
            WhyceId = request.WhyceId,
            PolicyId = request.PolicyId
        }, cancellationToken);
    }

    /// <summary>
    /// Synchronous route resolution (for composition/inspection only).
    /// </summary>
    public WorkflowRouteResult Route(WorkflowRouteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = _contextResolver.Resolve(
            request.Cluster,
            request.Subcluster,
            request.Domain,
            request.Context);

        var registration = _workflowResolver.Resolve(request.WorkflowId);

        return new WorkflowRouteResult(
            registration.WorkflowId,
            context,
            registration.Steps,
            true);
    }
}

public sealed record WorkflowRouteRequest(
    string WorkflowId,
    string Cluster,
    string Subcluster,
    string Domain,
    string Context);

public sealed record WorkflowRouteResult(
    string WorkflowId,
    WorkflowContext Context,
    IReadOnlyList<string> Steps,
    bool Resolved);
