namespace Whycespace.Domain.BusinessSystem.Document.Version;

public sealed class VersionAggregate
{
    public static VersionAggregate Create()
    {
        var aggregate = new VersionAggregate();
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
