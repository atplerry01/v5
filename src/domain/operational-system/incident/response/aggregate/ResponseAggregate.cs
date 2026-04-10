namespace Whycespace.Domain.OperationalSystem.Incident.Response;

public sealed class ResponseAggregate
{
    public static ResponseAggregate Create()
    {
        var aggregate = new ResponseAggregate();
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
