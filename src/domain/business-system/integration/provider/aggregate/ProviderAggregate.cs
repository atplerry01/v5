namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed class ProviderAggregate
{
    public static ProviderAggregate Create()
    {
        var aggregate = new ProviderAggregate();
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
