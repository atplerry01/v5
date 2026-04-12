namespace Whycespace.Domain.BusinessSystem.Entitlement.Limit;

public static class LimitErrors
{
    public static LimitDomainException MissingId()
        => new("LimitId is required and must not be empty.");

    public static LimitDomainException MissingSubjectId()
        => new("LimitSubjectId is required and must not be empty.");

    public static LimitDomainException InvalidThreshold()
        => new("ThresholdValue must be greater than zero.");

    public static LimitDomainException InvalidStateTransition(LimitStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static LimitDomainException AlreadyEnforced(LimitId id)
        => new($"Limit '{id.Value}' has already been enforced.");

    public static LimitDomainException AlreadyBreached(LimitId id)
        => new($"Limit '{id.Value}' has already been breached.");
}

public sealed class LimitDomainException : Exception
{
    public LimitDomainException(string message) : base(message) { }
}
