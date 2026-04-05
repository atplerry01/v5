namespace Whycespace.Domain.OrchestrationSystem.Workflow.Definition;

public sealed class DefinitionAggregate
{
    public static DefinitionAggregate Create()
    {
        var aggregate = new DefinitionAggregate();
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
