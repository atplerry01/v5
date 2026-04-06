namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Estimated impact of applying a governance recommendation.
/// </summary>
public sealed record ImpactEstimate
{
    public double EconomicScore { get; }
    public double OperationalScore { get; }
    public double GovernanceScore { get; }
    public double CompositeScore { get; }

    private ImpactEstimate(double economic, double operational, double governance)
    {
        EconomicScore = economic;
        OperationalScore = operational;
        GovernanceScore = governance;
        CompositeScore = (economic + operational + governance) / 3.0;
    }

    public static ImpactEstimate From(double economic, double operational, double governance)
    {
        ValidateScore(economic, nameof(economic));
        ValidateScore(operational, nameof(operational));
        ValidateScore(governance, nameof(governance));
        return new(economic, operational, governance);
    }

    private static void ValidateScore(double value, string name)
    {
        if (value < -1.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(name, $"Impact score must be between -1.0 and 1.0.");
    }
}
