namespace Whycespace.Domain.BusinessSystem.Notification.Channel;

public sealed class ChannelAggregate
{
    public static ChannelAggregate Create()
    {
        var aggregate = new ChannelAggregate();
        aggregate.ValidateBeforeChange();
        aggregate.EnsureInvariants();
        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    private void EnsureInvariants()
    {
        // Domain invariant checks enforced BEFORE any event is raised
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
    }
}
