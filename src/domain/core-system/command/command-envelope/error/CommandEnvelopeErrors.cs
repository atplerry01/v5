namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public static class CommandEnvelopeErrors
{
    public static InvalidOperationException MissingId() =>
        new("CommandEnvelopeId is required and must not be empty.");

    public static InvalidOperationException MissingMetadata() =>
        new("Command envelope must include valid metadata.");

    public static InvalidOperationException InvalidStateTransition(CommandEnvelopeStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
