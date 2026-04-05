namespace Whycespace.Domain.IntelligenceSystem.Relationship.Influence;

public sealed class InfluenceAggregate
{
    public static InfluenceAggregate Create()
    {
        var aggregate = new InfluenceAggregate();
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
