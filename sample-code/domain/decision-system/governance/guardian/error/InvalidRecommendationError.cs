namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

public static class InvalidRecommendationError
{
    public const string EmptyPolicies = "RECOMMENDATION_EMPTY_POLICIES";
    public const string InvalidConfidence = "RECOMMENDATION_INVALID_CONFIDENCE";
    public const string InvalidTransition = "RECOMMENDATION_INVALID_TRANSITION";
    public const string AlreadyTerminal = "RECOMMENDATION_ALREADY_TERMINAL";
    public const string MissingInsights = "RECOMMENDATION_MISSING_INSIGHTS";
}
