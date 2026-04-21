using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public static class ApprovalErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ApprovalId is required and must not be empty.");

    public static DomainException AlreadyApproved(ApprovalId id)
        => new DomainInvariantViolationException($"Approval '{id.Value}' has already been approved.");

    public static DomainException AlreadyRejected(ApprovalId id)
        => new DomainInvariantViolationException($"Approval '{id.Value}' has already been rejected.");

    public static DomainException InvalidStateTransition(ApprovalStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Approval has already been initialized.");
}
