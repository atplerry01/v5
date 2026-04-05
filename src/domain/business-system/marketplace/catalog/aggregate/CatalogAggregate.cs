namespace Whycespace.Domain.BusinessSystem.Marketplace.Catalog;

public sealed class CatalogAggregate
{
    public static CatalogAggregate Create()
    {
        var aggregate = new CatalogAggregate();
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
