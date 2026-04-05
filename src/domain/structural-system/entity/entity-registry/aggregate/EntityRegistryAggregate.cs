namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityRegistryAggregate
{
    public static EntityRegistryAggregate Create()
    {
        var aggregate = new EntityRegistryAggregate();
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
