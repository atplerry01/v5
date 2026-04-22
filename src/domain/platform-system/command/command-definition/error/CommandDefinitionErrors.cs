using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public static class CommandDefinitionErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("CommandDefinition has already been initialized.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("CommandDefinition is already deprecated.");

    public static DomainException MissingSchemaId() =>
        new DomainInvariantViolationException("CommandDefinition requires a non-empty schema ID.");

    public static DomainException InvalidOwnerRoute() =>
        new DomainInvariantViolationException("CommandDefinition requires a valid owner DomainRoute.");
}
