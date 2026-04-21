using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

public static class SystemVerificationErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("SystemVerification has already been initialized.");
}
