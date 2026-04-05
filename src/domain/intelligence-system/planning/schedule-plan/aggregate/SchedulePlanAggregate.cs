namespace Whycespace.Domain.IntelligenceSystem.Planning.SchedulePlan;

public sealed class SchedulePlanAggregate
{
    public static SchedulePlanAggregate Create()
    {
        var aggregate = new SchedulePlanAggregate();
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
