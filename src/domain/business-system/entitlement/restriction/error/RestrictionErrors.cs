namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public static class RestrictionErrors
{
    public static RestrictionDomainException MissingId()
        => new("RestrictionId is required and must not be empty.");

    public static RestrictionDomainException MissingSubjectId()
        => new("RestrictionSubjectId is required and must not be empty.");

    public static RestrictionDomainException MissingCondition()
        => new("ConditionDescription is required and must not be empty.");

    public static RestrictionDomainException InvalidStateTransition(RestrictionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RestrictionDomainException AlreadyViolated(RestrictionId id)
        => new($"Restriction '{id.Value}' has already been violated.");

    public static RestrictionDomainException AlreadyLifted(RestrictionId id)
        => new($"Restriction '{id.Value}' has already been lifted.");
}

public sealed class RestrictionDomainException : Exception
{
    public RestrictionDomainException(string message) : base(message) { }
}
