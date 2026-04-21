namespace Whycespace.Domain.OperationalSystem.Invariant.CommandIntegrity;

public sealed class CommandMustProduceDomainEventPolicy
{
    public CommandIntegrityDecision Decide(Guid commandId, int emittedEventCount)
    {
        if (commandId == Guid.Empty)
            return CommandIntegrityDecision.Deny(CommandIntegrityReason.MissingCommand);

        if (emittedEventCount <= 0)
            return CommandIntegrityDecision.Deny(CommandIntegrityReason.NoDomainEventEmitted);

        return CommandIntegrityDecision.Allow();
    }
}
