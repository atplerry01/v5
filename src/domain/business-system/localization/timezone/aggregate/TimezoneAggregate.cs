namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public sealed class TimezoneAggregate
{
    public static TimezoneAggregate Create()
    {
        var aggregate = new TimezoneAggregate();
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
