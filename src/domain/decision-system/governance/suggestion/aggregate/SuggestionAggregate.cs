namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

public sealed class SuggestionAggregate
{
    public static SuggestionAggregate Create()
    {
        var aggregate = new SuggestionAggregate();
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
