using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Performance;

public static class PerformanceErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Performance has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Performance Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Performance Descriptor must not be empty.");
}
