namespace Whycespace.Domain.BusinessSystem.Integration.Export;

public sealed class ExportAggregate
{
    public static ExportAggregate Create()
    {
        var aggregate = new ExportAggregate();
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
