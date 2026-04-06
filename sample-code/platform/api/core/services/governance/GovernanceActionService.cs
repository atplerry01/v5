using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Governance;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Platform.Api.Core.Services.Governance;

/// <summary>
/// WSS-routed governance action service.
/// Maps GovernanceActionRequest → downstream command and dispatches via DownstreamAdapter.
///
/// GUARANTEED FLOW:
///   Platform → DownstreamAdapter → ProcessHandler → WSS → Runtime → T0U Governance Engine
///
/// Platform does NOT decide. Platform only forwards.
/// WhycePolicy at runtime is the sole decision authority.
/// </summary>
public sealed class GovernanceActionService : IGovernanceActionService
{
    private static readonly HashSet<string> ValidActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "PROPOSE", "APPROVE", "REJECT"
    };

    private readonly DownstreamAdapter _downstream;

    public GovernanceActionService(DownstreamAdapter downstream)
    {
        _downstream = downstream;
    }

    public async Task<GovernanceActionResult> SubmitActionAsync(
        GovernanceActionRequest request,
        string whyceId,
        string correlationId,
        string traceId,
        CancellationToken cancellationToken = default)
    {
        // Validate action type
        if (!ValidActions.Contains(request.Action))
            return GovernanceActionResult.Failed(
                $"Invalid governance action '{request.Action}' — must be PROPOSE, APPROVE, or REJECT");

        // Derive deterministic workflow ID
        var workflowId = DeterministicIdHelper.FromSeed(
            $"governance:{correlationId}:{request.DecisionId}:{request.Action}");

        // Map action to downstream command type
        var commandType = $"governance.policy.{request.Action.ToLowerInvariant()}";

        // Dispatch via DownstreamAdapter → ProcessHandler → WSS → Runtime
        var response = await _downstream.SendCommandAsync(
            commandType: commandType,
            payload: new
            {
                DecisionId = request.DecisionId,
                Action = request.Action,
                Payload = request.Payload,
                Justification = request.Justification
            },
            correlationId: correlationId,
            whyceId: whyceId,
            traceId: traceId,
            cancellationToken: cancellationToken);

        if (response.StatusCode is >= 200 and < 300)
            return GovernanceActionResult.Accepted(workflowId);

        return GovernanceActionResult.Failed(
            response.Error ?? "Governance action dispatch failed");
    }
}
