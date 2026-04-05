namespace Whycespace.Domain.IntelligenceSystem.Search.Result;

public sealed class ResultAggregate
{
    public static ResultAggregate Create()
    {
        var aggregate = new ResultAggregate();
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
