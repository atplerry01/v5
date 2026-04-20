namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public static class ApprovalErrors
{
    public static ApprovalDomainException MissingId()
        => new("ApprovalId is required and must not be empty.");

    public static ApprovalDomainException AlreadyApproved(ApprovalId id)
        => new($"Approval '{id.Value}' has already been approved.");

    public static ApprovalDomainException AlreadyRejected(ApprovalId id)
        => new($"Approval '{id.Value}' has already been rejected.");

    public static ApprovalDomainException InvalidStateTransition(ApprovalStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ApprovalDomainException : Exception
{
    public ApprovalDomainException(string message) : base(message) { }
}
