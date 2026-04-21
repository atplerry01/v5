using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Temporal.ScheduleReference;

public static class ScheduleReferenceErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ScheduleReference has already been initialized.");
}
