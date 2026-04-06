namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed class TrustService
{
    public bool MeetsMinimumTrust(TrustProfileAggregate profile, TrustLevel requiredLevel)
    {
        var scoreThreshold = requiredLevel switch
        {
            _ when requiredLevel == TrustLevel.Unverified => 0m,
            _ when requiredLevel == TrustLevel.Low => 20m,
            _ when requiredLevel == TrustLevel.Medium => 40m,
            _ when requiredLevel == TrustLevel.High => 60m,
            _ when requiredLevel == TrustLevel.Trusted => 80m,
            _ => 100m
        };
        return profile.Score.Value >= scoreThreshold;
    }
}
