using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Workforce;

public static class WorkforceErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Workforce has already been initialized.");

    public static DomainException MissingId()
        => new DomainInvariantViolationException("Workforce Id must not be empty.");

    public static DomainException MissingDescriptor()
        => new DomainInvariantViolationException("Workforce Descriptor must not be empty.");
}
