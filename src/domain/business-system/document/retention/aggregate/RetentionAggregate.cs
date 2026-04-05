namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public sealed class RetentionAggregate
{
    public static RetentionAggregate Create()
    {
        var aggregate = new RetentionAggregate();
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
