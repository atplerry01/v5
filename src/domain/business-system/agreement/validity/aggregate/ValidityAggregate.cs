namespace Whycespace.Domain.BusinessSystem.Agreement.Validity;

public sealed class ValidityAggregate
{
    public static ValidityAggregate Create()
    {
        var aggregate = new ValidityAggregate();
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
