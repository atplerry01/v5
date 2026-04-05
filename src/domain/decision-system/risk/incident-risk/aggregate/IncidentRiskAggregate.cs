namespace Whycespace.Domain.DecisionSystem.Risk.IncidentRisk;

public sealed class IncidentRiskAggregate
{
    public static IncidentRiskAggregate Create()
    {
        var aggregate = new IncidentRiskAggregate();
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
