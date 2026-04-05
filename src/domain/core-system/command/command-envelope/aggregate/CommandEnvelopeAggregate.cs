namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed class CommandEnvelopeAggregate
{
    public static CommandEnvelopeAggregate Create()
    {
        var aggregate = new CommandEnvelopeAggregate();
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
