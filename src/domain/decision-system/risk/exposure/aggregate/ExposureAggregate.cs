namespace Whycespace.Domain.DecisionSystem.Risk.Exposure;

public sealed class ExposureAggregate
{
    public static ExposureAggregate Create()
    {
        var aggregate = new ExposureAggregate();
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
