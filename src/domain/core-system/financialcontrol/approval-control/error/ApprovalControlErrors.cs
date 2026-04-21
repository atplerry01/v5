using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.ApprovalControl;

public static class ApprovalControlErrors
{
    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("ApprovalControl has already been initialized.");
}
