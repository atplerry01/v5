namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Estimated risk of applying a governance recommendation. Range: 0 (no risk) to 100 (maximum risk).
/// </summary>
public sealed record RiskEstimate
{
    public int Score { get; }
    public string Category { get; }

    private RiskEstimate(int score, string category)
    {
        Score = score;
        Category = category;
    }

    public static RiskEstimate From(int score)
    {
        if (score < 0 || score > 100)
            throw new ArgumentOutOfRangeException(nameof(score), "Risk score must be between 0 and 100.");

        var category = score switch
        {
            <= 25 => "Low",
            <= 50 => "Medium",
            <= 75 => "High",
            _ => "Critical"
        };

        return new(score, category);
    }

    public bool IsCritical => Score > 75;
}
