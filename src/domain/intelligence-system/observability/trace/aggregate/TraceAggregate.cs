namespace Whycespace.Domain.IntelligenceSystem.Observability.Trace;

public sealed class TraceAggregate
{
    public static TraceAggregate Create()
    {
        var aggregate = new TraceAggregate();
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
