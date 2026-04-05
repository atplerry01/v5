namespace Whycespace.Domain.BusinessSystem.Localization.Locale;

public sealed class LocaleAggregate
{
    public static LocaleAggregate Create()
    {
        var aggregate = new LocaleAggregate();
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
