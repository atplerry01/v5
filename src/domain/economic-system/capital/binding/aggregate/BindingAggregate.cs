namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class BindingAggregate
{
    public static BindingAggregate Create()
    {
        var aggregate = new BindingAggregate();
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
