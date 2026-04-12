namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(ReplayStatus status)
    {
        return status == ReplayStatus.Active;
    }
}
