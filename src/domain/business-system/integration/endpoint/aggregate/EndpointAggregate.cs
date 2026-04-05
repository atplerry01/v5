namespace Whycespace.Domain.BusinessSystem.Integration.Endpoint;

public sealed class EndpointAggregate
{
    public static EndpointAggregate Create()
    {
        var aggregate = new EndpointAggregate();
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
