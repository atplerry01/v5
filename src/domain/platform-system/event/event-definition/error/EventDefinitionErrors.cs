using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public static class EventDefinitionErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("EventDefinition has already been initialized.");

    public static DomainException SchemaIdMissing() =>
        new DomainInvariantViolationException("EventDefinition requires a non-empty SchemaId.");

    public static DomainException SourceRouteMissing() =>
        new DomainInvariantViolationException("EventDefinition requires a valid SourceRoute.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("EventDefinition has already been deprecated.");
}
