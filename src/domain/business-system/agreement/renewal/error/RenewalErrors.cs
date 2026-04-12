namespace Whycespace.Domain.BusinessSystem.Agreement.Renewal;

public static class RenewalErrors
{
    public static RenewalDomainException MissingId()
        => new("RenewalId is required and must not be empty.");

    public static RenewalDomainException MissingSourceId()
        => new("RenewalSourceId is required and must not be empty.");

    public static RenewalDomainException AlreadyRenewed(RenewalId id)
        => new($"Renewal '{id.Value}' has already been renewed.");

    public static RenewalDomainException AlreadyExpired(RenewalId id)
        => new($"Renewal '{id.Value}' has already expired. Cannot renew an expired entity.");

    public static RenewalDomainException InvalidStateTransition(RenewalStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class RenewalDomainException : Exception
{
    public RenewalDomainException(string message) : base(message) { }
}
