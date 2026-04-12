namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(ReplayStatus status)
    {
        return status == ReplayStatus.Active;
    }
}
