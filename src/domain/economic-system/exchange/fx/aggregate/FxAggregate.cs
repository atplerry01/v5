namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed class FxAggregate
{
    public static FxAggregate Create()
    {
        var aggregate = new FxAggregate();
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
