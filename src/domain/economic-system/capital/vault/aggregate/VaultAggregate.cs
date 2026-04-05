namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class VaultAggregate
{
    public static VaultAggregate Create()
    {
        var aggregate = new VaultAggregate();
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
