namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

/// <summary>
/// Confidence level attached to a governance suggestion.
/// </summary>
public sealed record SuggestionConfidence
{
    public decimal Value { get; }
    public bool IsActionable => Value >= 0.8m;
    public bool IsReliable => Value >= 0.7m;

    private SuggestionConfidence(decimal value) => Value = value;

    public static SuggestionConfidence From(decimal value) =>
        value < 0m || value > 1m
            ? throw new ArgumentOutOfRangeException(nameof(value), "Confidence must be between 0.0 and 1.0.")
            : new(value);
}
