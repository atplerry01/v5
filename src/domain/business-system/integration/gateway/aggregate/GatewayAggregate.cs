namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public sealed class GatewayAggregate
{
    public static GatewayAggregate Create()
    {
        var aggregate = new GatewayAggregate();
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
