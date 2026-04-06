namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

/// <summary>
/// Specification: suggestion must be approved and have actionable confidence.
/// </summary>
public sealed class ActionableSuggestionSpecification
{
    public bool IsSatisfiedBy(GovernanceSuggestionAggregate suggestion) =>
        suggestion.Status == SuggestionStatus.Approved && suggestion.Confidence.IsActionable;
}
