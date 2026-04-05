namespace Whycespace.Domain.IntelligenceSystem.Relationship.TrustNetwork;

public sealed class TrustNetworkAggregate
{
    public static TrustNetworkAggregate Create()
    {
        var aggregate = new TrustNetworkAggregate();
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
