using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Governance.Suggestion;

/// <summary>
/// Suggestion approval workflow — composition only.
/// Orchestrates the suggestion lifecycle: intelligence proposes,
/// guardians review, quorum approves, runtime activates.
/// NO execution, NO domain mutation, NO persistence.
///
/// Boundary declaration:
/// - BCs touched: decision-system/governance/suggestion
/// - Engines composed: none directly — dispatches via runtime
/// - Runtime pipelines: governance + policy middleware
/// </summary>
public sealed class SuggestionApprovalWorkflow
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public SuggestionApprovalWorkflow(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    /// <summary>
    /// Submits a suggestion for guardian review.
    /// </summary>
    public async Task<IntentResult> SubmitForReviewAsync(
        string suggestionId,
        string reviewerId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "governance.suggestion.review",
            Payload = new { SuggestionId = suggestionId, ReviewerId = reviewerId },
            CorrelationId = correlationId,
            Timestamp = default
        }, cancellationToken);
    }

    /// <summary>
    /// Submits a reviewed suggestion for approval.
    /// Requires guardian quorum for policy-changing suggestions.
    /// </summary>
    public async Task<IntentResult> SubmitForApprovalAsync(
        string suggestionId,
        string approverId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "governance.suggestion.approve",
            Payload = new { SuggestionId = suggestionId, ApproverId = approverId },
            CorrelationId = correlationId,
            Timestamp = default
        }, cancellationToken);
    }

    /// <summary>
    /// Activates an approved suggestion. This triggers the actual
    /// policy/configuration change through the runtime pipeline.
    /// </summary>
    public async Task<IntentResult> ActivateAsync(
        string suggestionId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "governance.suggestion.activate",
            Payload = new { SuggestionId = suggestionId },
            CorrelationId = correlationId,
            Timestamp = default
        }, cancellationToken);
    }
}
