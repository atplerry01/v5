namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed class TrustThresholdSpec : Specification<TrustProfileAggregate>
{
    private readonly TrustLevel _minimumLevel;

    public TrustThresholdSpec(TrustLevel minimumLevel) => _minimumLevel = minimumLevel;

    public override bool IsSatisfiedBy(TrustProfileAggregate entity)
    {
        var threshold = _minimumLevel switch
        {
            _ when _minimumLevel == TrustLevel.Low => 20m,
            _ when _minimumLevel == TrustLevel.Medium => 40m,
            _ when _minimumLevel == TrustLevel.High => 60m,
            _ when _minimumLevel == TrustLevel.Trusted => 80m,
            _ => 0m
        };
        return entity.Status == TrustProfileStatus.Active && entity.Score.Value >= threshold;
    }
}
