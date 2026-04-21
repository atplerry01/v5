using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public static class CommandDefinitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("CommandDefinitionId is required and must not be empty.");

    public static DomainException MissingSchema()
        => new DomainInvariantViolationException("Command definition must include a valid schema.");

    public static DomainException InvalidStateTransition(CommandDefinitionStatus current, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{current}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("CommandDefinition has already been initialized.");
}
