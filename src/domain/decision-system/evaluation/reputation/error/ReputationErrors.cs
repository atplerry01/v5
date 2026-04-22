using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Reputation;

public static class ReputationErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Reputation has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Reputation Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Reputation Descriptor must not be empty.");
}
