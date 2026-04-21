namespace Whycespace.Domain.OperationalSystem.Invariant.CommandIntegrity;

public enum CommandIntegrityOutcome
{
    Allowed,
    Denied
}

public enum CommandIntegrityReason
{
    None,
    MissingCommand,
    NoDomainEventEmitted
}

public readonly record struct CommandIntegrityDecision
{
    public CommandIntegrityOutcome Outcome { get; }
    public CommandIntegrityReason Reason { get; }

    private CommandIntegrityDecision(CommandIntegrityOutcome outcome, CommandIntegrityReason reason)
    {
        Outcome = outcome;
        Reason = reason;
    }

    public static CommandIntegrityDecision Allow()
        => new(CommandIntegrityOutcome.Allowed, CommandIntegrityReason.None);

    public static CommandIntegrityDecision Deny(CommandIntegrityReason reason)
        => new(CommandIntegrityOutcome.Denied, reason);

    public bool IsAllowed => Outcome == CommandIntegrityOutcome.Allowed;
}
