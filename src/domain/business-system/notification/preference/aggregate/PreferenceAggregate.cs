namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public sealed class PreferenceAggregate
{
    public static PreferenceAggregate Create()
    {
        var aggregate = new PreferenceAggregate();
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
