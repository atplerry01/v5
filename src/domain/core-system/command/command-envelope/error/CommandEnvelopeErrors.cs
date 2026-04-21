using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public static class CommandEnvelopeErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CommandEnvelopeId is required and must not be empty.");

    public static DomainException MissingMetadata()
        => new DomainInvariantViolationException("Command envelope must include valid metadata.");

    public static DomainException InvalidStateTransition(CommandEnvelopeStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("CommandEnvelope has already been initialized.");
}
