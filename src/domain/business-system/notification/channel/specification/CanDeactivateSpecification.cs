namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(ChannelStatus status)
    {
        return status == ChannelStatus.Active;
    }
}
