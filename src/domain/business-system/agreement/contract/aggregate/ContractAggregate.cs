namespace Whycespace.Domain.BusinessSystem.Agreement.Contract;

public sealed class ContractAggregate
{
    public static ContractAggregate Create()
    {
        var aggregate = new ContractAggregate();
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
