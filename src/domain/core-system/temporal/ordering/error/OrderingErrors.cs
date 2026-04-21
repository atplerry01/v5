using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

public static class OrderingErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Ordering has already been initialized.");
}
