namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Specification: Does a trust level meet the required threshold?
/// Invariant: trust must be within [0, 100] bounds.
/// </summary>
public sealed class FederationTrustThresholdSpecification
{
    private readonly decimal _threshold;

    public FederationTrustThresholdSpecification(decimal threshold)
    {
        if (threshold < 0 || threshold > 100)
            throw new ArgumentOutOfRangeException(nameof(threshold),
                $"Threshold must be between 0 and 100, got {threshold}.");
        _threshold = threshold;
    }

    public bool IsSatisfiedBy(TrustLevel trustLevel) =>
        trustLevel.MeetsThreshold(_threshold);

    public bool IsSatisfiedBy(FederationTrustAggregate trust) =>
        trust.AdjustedTrustScore.MeetsThreshold(_threshold);

    public static FederationTrustThresholdSpecification MinimumForApproval => new(25m);
    public static FederationTrustThresholdSpecification MinimumForLinking => new(50m);
}
