namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed class RecurrenceAggregate
{
    public static RecurrenceAggregate Create()
    {
        var aggregate = new RecurrenceAggregate();
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
