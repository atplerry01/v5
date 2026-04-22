using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Incentive;

public static class IncentiveErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Incentive has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Incentive Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Incentive Descriptor must not be empty.");
}
