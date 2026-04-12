namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public static class EnforcementErrors
{
    public static EnforcementDomainException MissingId()
        => new("EnforcementId is required and must not be empty.");

    public static EnforcementDomainException MissingAction()
        => new("EnforcementAction is required.");

    public static EnforcementDomainException InvalidStateTransition(EnforcementStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class EnforcementDomainException : InvalidOperationException
{
    public EnforcementDomainException(string message) : base(message) { }
}
