namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

/// <summary>
/// Domain service for suggestion lifecycle evaluation.
/// Stateless — all data passed as parameters.
/// </summary>
public sealed class SuggestionLifecycleService
{
    public bool CanApprove(GovernanceSuggestionAggregate suggestion) =>
        suggestion.Status == SuggestionStatus.Reviewed && suggestion.Confidence.IsActionable;

    public bool CanActivate(GovernanceSuggestionAggregate suggestion) =>
        suggestion.Status == SuggestionStatus.Approved;

    public bool RequiresGuardianQuorum(GovernanceSuggestionAggregate suggestion) =>
        suggestion.SuggestionType is "PolicyChange" or "ThresholdAdjustment" or "JurisdictionOverlay";
}
