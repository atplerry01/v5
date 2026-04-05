namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed class RequestAggregate
{
    public static RequestAggregate Create()
    {
        var aggregate = new RequestAggregate();
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
