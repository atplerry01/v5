namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

public sealed class TimeWindowAggregate
{
    public static TimeWindowAggregate Create()
    {
        var aggregate = new TimeWindowAggregate();
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
