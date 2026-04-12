namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public static class CommandRoutingErrors
{
    public static InvalidOperationException MissingId() =>
        new("CommandRoutingId is required and must not be empty.");

    public static InvalidOperationException MissingRoutingRule() =>
        new("Command routing must include a valid routing rule.");

    public static InvalidOperationException InvalidStateTransition(CommandRoutingStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
