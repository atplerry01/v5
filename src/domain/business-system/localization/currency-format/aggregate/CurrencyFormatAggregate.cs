namespace Whycespace.Domain.BusinessSystem.Localization.CurrencyFormat;

public sealed class CurrencyFormatAggregate
{
    public static CurrencyFormatAggregate Create()
    {
        var aggregate = new CurrencyFormatAggregate();
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
