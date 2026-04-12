namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ReplayStatus status)
    {
        return status is ReplayStatus.Defined or ReplayStatus.Disabled;
    }
}
