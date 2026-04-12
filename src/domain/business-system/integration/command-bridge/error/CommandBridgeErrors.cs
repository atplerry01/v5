namespace Whycespace.Domain.BusinessSystem.Integration.CommandBridge;

public static class CommandBridgeErrors
{
    public static CommandBridgeDomainException MissingId()
        => new("CommandBridgeId is required and must not be empty.");

    public static CommandBridgeDomainException MissingMappingId()
        => new("CommandMappingId is required and must not be empty.");

    public static CommandBridgeDomainException InvalidStateTransition(CommandBridgeStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CommandBridgeDomainException AlreadyActive(CommandBridgeId id)
        => new($"CommandBridge '{id.Value}' is already active.");

    public static CommandBridgeDomainException AlreadyDisabled(CommandBridgeId id)
        => new($"CommandBridge '{id.Value}' is already disabled.");
}

public sealed class CommandBridgeDomainException : Exception
{
    public CommandBridgeDomainException(string message) : base(message) { }
}
