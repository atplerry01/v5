using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public static class SchemaDefinitionErrors
{
    public static DomainException AlreadyInitialized() =>
        new DomainInvariantViolationException("SchemaDefinition has already been initialized.");

    public static DomainException FieldsRequired() =>
        new DomainInvariantViolationException("SchemaDefinition requires at least one field.");

    public static DomainException AlreadyPublished() =>
        new DomainInvariantViolationException("SchemaDefinition has already been published.");

    public static DomainException AlreadyDeprecated() =>
        new DomainInvariantViolationException("SchemaDefinition is deprecated and cannot be modified.");

    public static DomainException NotInDraftState() =>
        new DomainInvariantViolationException("SchemaDefinition must be in Draft state to publish.");
}
