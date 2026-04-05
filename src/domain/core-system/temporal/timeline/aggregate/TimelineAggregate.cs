namespace Whycespace.Domain.CoreSystem.Temporal.Timeline;

public sealed class TimelineAggregate
{
    public static TimelineAggregate Create()
    {
        var aggregate = new TimelineAggregate();
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
