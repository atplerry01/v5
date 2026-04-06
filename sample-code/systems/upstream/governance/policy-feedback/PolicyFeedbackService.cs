using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Governance.PolicyFeedback;

/// <summary>
/// Policy feedback loop — composition only.
/// Turns violation trends into governance inputs by dispatching
/// analysis intents to T3I intelligence engines.
/// NO execution, NO domain mutation, NO persistence.
/// </summary>
public sealed class PolicyFeedbackService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public PolicyFeedbackService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    /// <summary>
    /// Dispatches a violation trend analysis request to T3I.
    /// </summary>
    public async Task<IntentResult> AnalyzeViolationTrendsAsync(
        string policyId,
        int recentViolationCount,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "intelligence.policy.violation-trend-analysis",
            Payload = new { PolicyId = policyId, ViolationCount = recentViolationCount },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-feedback-source"] = "policy-enforcement" }
        }, cancellationToken);
    }

    /// <summary>
    /// Dispatches a policy suggestion generation request based on analysis results.
    /// </summary>
    public async Task<IntentResult> GeneratePolicySuggestionAsync(
        string analysisId,
        string suggestedAction,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "governance.suggestion.propose",
            Payload = new { AnalysisId = analysisId, SuggestedAction = suggestedAction },
            CorrelationId = correlationId,
            Timestamp = default
        }, cancellationToken);
    }
}
