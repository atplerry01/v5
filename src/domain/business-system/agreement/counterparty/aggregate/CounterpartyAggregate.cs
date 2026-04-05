namespace Whycespace.Domain.BusinessSystem.Agreement.Counterparty;

public sealed class CounterpartyAggregate
{
    public static CounterpartyAggregate Create()
    {
        var aggregate = new CounterpartyAggregate();
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
