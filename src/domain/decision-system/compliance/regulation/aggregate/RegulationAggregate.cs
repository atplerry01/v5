namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

public sealed class RegulationAggregate
{
    public static RegulationAggregate Create()
    {
        var aggregate = new RegulationAggregate();
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
