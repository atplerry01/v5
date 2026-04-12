namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ChannelStatus status)
    {
        return status == ChannelStatus.Draft;
    }
}
