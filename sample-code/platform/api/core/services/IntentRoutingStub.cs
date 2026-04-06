using Whycespace.Platform.Api.Core.Contracts;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Registry-backed intent routing service.
/// Resolves ClassifiedIntent → IntentRoute using the IntentRoutingRegistry.
/// Enforces Whycespace cluster taxonomy (Whyce + Name) and WSS execution target.
///
/// ZERO business logic. ZERO runtime calls. ZERO engine calls.
/// Pure registry lookup + validation.
/// </summary>
public sealed class IntentRoutingService : IIntentRoutingService
{
    private readonly IntentRoutingRegistry _registry;

    public IntentRoutingService(IntentRoutingRegistry registry)
    {
        _registry = registry;
    }

    public Task<IntentRoutingResult> ResolveRouteAsync(ClassifiedIntent intent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(intent.WorkflowKey))
            return Task.FromResult(IntentRoutingResult.Fail(
                "WorkflowKey is required for routing", "MISSING_WORKFLOW_KEY"));

        var definition = _registry.Resolve(intent.WorkflowKey);

        if (definition is null)
            return Task.FromResult(IntentRoutingResult.Fail(
                $"No route found for workflow key: {intent.WorkflowKey}", "UNKNOWN_WORKFLOW_KEY"));

        // Enforce cluster taxonomy
        if (!IntentRoutingRegistry.IsValidClusterName(definition.Cluster))
            return Task.FromResult(IntentRoutingResult.Fail(
                $"Invalid cluster name '{definition.Cluster}' — must start with '{IntentRoutingRegistry.ClusterPrefix}'",
                "INVALID_CLUSTER_NAME"));

        // Enforce execution target
        if (!string.Equals(definition.ExecutionTarget, IntentRoutingRegistry.ExecutionTargetWss, StringComparison.Ordinal))
            return Task.FromResult(IntentRoutingResult.Fail(
                $"Invalid execution target '{definition.ExecutionTarget}' — must be '{IntentRoutingRegistry.ExecutionTargetWss}'",
                "INVALID_EXECUTION_TARGET"));

        // Enforce complete route
        if (string.IsNullOrWhiteSpace(definition.Authority) || string.IsNullOrWhiteSpace(definition.SubCluster))
            return Task.FromResult(IntentRoutingResult.Fail(
                "Incomplete route: Authority and SubCluster are required", "INCOMPLETE_ROUTE"));

        var route = new IntentRoute
        {
            Cluster = definition.Cluster,
            Authority = definition.Authority,
            SubCluster = definition.SubCluster,
            WorkflowKey = definition.WorkflowKey,
            ExecutionTarget = definition.ExecutionTarget,
            Domain = definition.Domain,
            CommandType = definition.WorkflowKey
        };

        return Task.FromResult(IntentRoutingResult.Ok(route));
    }
}
