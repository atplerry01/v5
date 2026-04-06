using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Governance;

/// <summary>
/// E18.7.8 — Automated governance trigger.
/// Monitors intelligence signals and triggers governance workflows
/// when risk thresholds are exceeded.
///
/// CRITICAL: Triggers workflow execution — does NOT make governance decisions.
/// Policy remains the only authority.
/// </summary>
public sealed class ClusterGovernanceTrigger
{
    private readonly IWorkflowRouter _router;
    private readonly IClock _clock;

    private const decimal HighRiskThreshold = 0.8m;
    private const decimal MediumRiskThreshold = 0.5m;

    public ClusterGovernanceTrigger(IWorkflowRouter router, IClock clock)
    {
        _router = router;
        _clock = clock;
    }

    public async Task<GovernanceTriggerResult> EvaluateAndTriggerAsync(
        GovernanceTriggerInput signal,
        string correlationId,
        string cluster,
        string subcluster,
        CancellationToken ct = default)
    {
        if (signal.RiskScore <= MediumRiskThreshold)
            return GovernanceTriggerResult.NoAction("Risk below threshold");

        var decisionType = signal.RiskScore > HighRiskThreshold
            ? "EMERGENCY_REVIEW"
            : "RISK_REVIEW";

        var request = new WorkflowDispatchRequest
        {
            WorkflowId = "CLUSTER_GOVERNANCE_DECISION_V1",
            CommandType = "GovernanceDecision",
            Payload = new
            {
                signal.ClusterId,
                signal.RiskScore,
                signal.Recommendation,
                DecisionType = decisionType
            },
            CorrelationId = correlationId,
            Cluster = cluster,
            Subcluster = subcluster,
            Domain = "governance",
            Context = "cluster.governance",
            Timestamp = _clock.UtcNowOffset
        };

        var result = await _router.RouteAsync(request, ct);

        return result.Success
            ? GovernanceTriggerResult.WorkflowTriggered(decisionType)
            : GovernanceTriggerResult.Failed(result.ErrorMessage ?? "Workflow dispatch failed");
    }
}

/// <summary>
/// Input DTO for the governance trigger — runtime-layer contract.
/// Decoupled from T3I GovernanceSignal to avoid cross-layer dependency.
/// </summary>
public sealed record GovernanceTriggerInput
{
    public required decimal RiskScore { get; init; }
    public required string Recommendation { get; init; }
    public required Guid ClusterId { get; init; }
    public required string DecisionType { get; init; }
}

public sealed record GovernanceTriggerResult
{
    public bool WasTriggered { get; init; }
    public string? DecisionType { get; init; }
    public string? Message { get; init; }

    public static GovernanceTriggerResult NoAction(string reason) => new()
    {
        WasTriggered = false,
        Message = reason
    };

    public static GovernanceTriggerResult WorkflowTriggered(string decisionType) => new()
    {
        WasTriggered = true,
        DecisionType = decisionType
    };

    public static GovernanceTriggerResult Failed(string error) => new()
    {
        WasTriggered = false,
        Message = error
    };
}
