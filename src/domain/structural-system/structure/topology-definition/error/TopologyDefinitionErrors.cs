using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public static class TopologyDefinitionErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("TopologyDefinition ID must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("TopologyDefinition descriptor must have non-empty name and kind.");

    public static DomainException InvalidStateTransition(TopologyDefinitionStatus status, string action)
        => new DomainInvariantViolationException($"Cannot perform '{action}' when topology-definition status is '{status}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("TopologyDefinition has already been initialized.");
}
