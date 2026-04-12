namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public static class RetentionErrors
{
    public static RetentionDomainException MissingId()
        => new("RetentionId is required and must not be empty.");

    public static RetentionDomainException InvalidStateTransition(RetentionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RetentionDomainException RetentionConditionNotMet()
        => new("Cannot expire before retention condition has been met.");
}
