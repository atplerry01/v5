namespace Whycespace.Domain.BusinessSystem.Resource.AssetResource;

public sealed class AssetResourceAggregate
{
    public static AssetResourceAggregate Create()
    {
        var aggregate = new AssetResourceAggregate();
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
