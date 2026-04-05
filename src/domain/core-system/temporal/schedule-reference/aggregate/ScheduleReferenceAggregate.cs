namespace Whycespace.Domain.CoreSystem.Temporal.ScheduleReference;

public sealed class ScheduleReferenceAggregate
{
    public static ScheduleReferenceAggregate Create()
    {
        var aggregate = new ScheduleReferenceAggregate();
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
