namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public sealed class CompletionAggregate
{
    public static CompletionAggregate Create()
    {
        var aggregate = new CompletionAggregate();
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
