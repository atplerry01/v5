namespace Whycespace.Domain.BusinessSystem.Integration.EventBridge;

public sealed class EventBridgeAggregate
{
    public static EventBridgeAggregate Create()
    {
        var aggregate = new EventBridgeAggregate();
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
