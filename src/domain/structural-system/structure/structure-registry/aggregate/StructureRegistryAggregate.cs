namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed class StructureRegistryAggregate
{
    public static StructureRegistryAggregate Create()
    {
        var aggregate = new StructureRegistryAggregate();
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
