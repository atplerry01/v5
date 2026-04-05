namespace Whycespace.Domain.OrchestrationSystem.Workflow.Template;

public sealed class TemplateAggregate
{
    public static TemplateAggregate Create()
    {
        var aggregate = new TemplateAggregate();
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
