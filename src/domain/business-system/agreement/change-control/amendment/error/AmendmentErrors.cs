namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public static class AmendmentErrors
{
    public static AmendmentDomainException MissingId()
        => new("AmendmentId is required and must not be empty.");

    public static AmendmentDomainException MissingTargetId()
        => new("AmendmentTargetId is required and must not be empty.");

    public static AmendmentDomainException AlreadyApplied(AmendmentId id)
        => new($"Amendment '{id.Value}' has already been applied.");

    public static AmendmentDomainException AlreadyReverted(AmendmentId id)
        => new($"Amendment '{id.Value}' has already been reverted.");

    public static AmendmentDomainException InvalidStateTransition(AmendmentStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class AmendmentDomainException : Exception
{
    public AmendmentDomainException(string message) : base(message) { }
}
