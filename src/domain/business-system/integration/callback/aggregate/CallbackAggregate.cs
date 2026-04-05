namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed class CallbackAggregate
{
    public static CallbackAggregate Create()
    {
        var aggregate = new CallbackAggregate();
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
