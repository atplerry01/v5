namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed class CommandRoutingAggregate
{
    public static CommandRoutingAggregate Create()
    {
        var aggregate = new CommandRoutingAggregate();
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
