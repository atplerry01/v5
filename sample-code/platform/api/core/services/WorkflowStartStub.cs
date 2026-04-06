using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// WSS workflow start service.
/// Dispatches workflow execution ONLY via DownstreamAdapter → ProcessHandler → WSS → Runtime.
///
/// GUARANTEED FLOW:
///   Platform → DownstreamAdapter → ProcessHandler → WorkflowRouter (WSS) → Runtime → Engines
///
/// MUST NOT:
/// - Call runtime directly
/// - Call engines
/// - Execute business logic
/// - Generate non-deterministic IDs
/// </summary>
public sealed class WorkflowStartService : IWorkflowStartService
{
    private readonly DownstreamAdapter _downstream;

    public WorkflowStartService(DownstreamAdapter downstream)
    {
        _downstream = downstream;
    }

    public async Task<WorkflowStartResult> StartAsync(
        WorkflowStartRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: ExecutionTarget must be WSS
        if (!string.Equals(request.ExecutionTarget, "wss", StringComparison.OrdinalIgnoreCase))
        {
            return WorkflowStartResult.Failed(
                request.WorkflowId,
                $"Invalid execution target '{request.ExecutionTarget}' — must be 'wss'",
                "INVALID_EXECUTION_TARGET");
        }

        // Guard: WorkflowKey must exist
        if (string.IsNullOrWhiteSpace(request.WorkflowKey))
        {
            return WorkflowStartResult.Failed(
                request.WorkflowId,
                "WorkflowKey is required",
                "MISSING_WORKFLOW_KEY");
        }

        // Dispatch via DownstreamAdapter → ProcessHandler → WSS → Runtime
        var response = await _downstream.SendCommandAsync(
            commandType: request.CommandType,
            payload: request.Payload,
            correlationId: request.CorrelationId,
            whyceId: request.WhyceId,
            traceId: request.TraceId,
            aggregateId: null,
            cancellationToken: cancellationToken);

        if (response.StatusCode is >= 200 and < 300)
        {
            return WorkflowStartResult.Accepted(request.WorkflowId);
        }

        return WorkflowStartResult.Failed(
            request.WorkflowId,
            response.Error ?? "Workflow dispatch failed",
            response.StatusCode.ToString());
    }
}
