namespace Whycespace.Domain.BusinessSystem.Integration.Handshake;

public sealed class HandshakeAggregate
{
    public static HandshakeAggregate Create()
    {
        var aggregate = new HandshakeAggregate();
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
