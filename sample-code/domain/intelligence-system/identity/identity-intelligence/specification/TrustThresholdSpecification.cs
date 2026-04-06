namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Specification that checks if a trust score meets a minimum threshold.
/// </summary>
public sealed class TrustThresholdSpecification
{
    private readonly decimal _minimumScore;

    public TrustThresholdSpecification(decimal minimumScore)
    {
        _minimumScore = minimumScore;
    }

    public bool IsSatisfiedBy(TrustScore score) => score.Value >= _minimumScore;

    public static TrustThresholdSpecification HighTrust => new(75m);
    public static TrustThresholdSpecification MediumTrust => new(50m);
    public static TrustThresholdSpecification LowTrust => new(25m);
}
