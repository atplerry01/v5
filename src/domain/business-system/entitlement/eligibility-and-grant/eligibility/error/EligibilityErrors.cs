namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public static class EligibilityErrors
{
    public static EligibilityDomainException MissingId()
        => new("EligibilityId is required and must not be empty.");

    public static EligibilityDomainException MissingSubject()
        => new("Eligibility requires a subject ref.");

    public static EligibilityDomainException MissingTarget()
        => new("Eligibility requires a target ref.");

    public static EligibilityDomainException InvalidStateTransition(EligibilityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static EligibilityDomainException AlreadyEvaluated(EligibilityId id, EligibilityStatus status)
        => new($"Eligibility '{id.Value}' is already evaluated ({status}).");
}

public sealed class EligibilityDomainException : Exception
{
    public EligibilityDomainException(string message) : base(message) { }
}
