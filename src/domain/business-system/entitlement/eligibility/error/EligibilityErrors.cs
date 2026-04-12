namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public static class EligibilityErrors
{
    public static EligibilityDomainException MissingId()
        => new("EligibilityId is required and must not be empty.");

    public static EligibilityDomainException MissingSubjectId()
        => new("SubjectId is required and must not be empty.");

    public static EligibilityDomainException MissingCriteria()
        => new("CriteriaDescription is required and must not be empty.");

    public static EligibilityDomainException InvalidStateTransition(EligibilityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static EligibilityDomainException AlreadyEvaluated(EligibilityId id)
        => new($"Eligibility '{id.Value}' has already been evaluated.");
}

public sealed class EligibilityDomainException : Exception
{
    public EligibilityDomainException(string message) : base(message) { }
}
