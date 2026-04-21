using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public static class TopologyErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("TopologyId is required and must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("TopologyDescriptor is required and must not be default.");

    public static DomainException InvalidStateTransition(TopologyStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Topology has already been initialized.");
}
