namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAggregate
{
    public static IncidentAggregate Create()
    {
        var aggregate = new IncidentAggregate();
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
