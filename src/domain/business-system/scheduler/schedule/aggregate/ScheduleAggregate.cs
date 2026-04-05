namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class ScheduleAggregate
{
    public static ScheduleAggregate Create()
    {
        var aggregate = new ScheduleAggregate();
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
