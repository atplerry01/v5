namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;

public static class AcceptanceErrors
{
    public static AcceptanceDomainException MissingId()
        => new("AcceptanceId is required and must not be empty.");

    public static AcceptanceDomainException AlreadyAccepted(AcceptanceId id)
        => new($"Acceptance '{id.Value}' has already been accepted.");

    public static AcceptanceDomainException AlreadyRejected(AcceptanceId id)
        => new($"Acceptance '{id.Value}' has already been rejected.");

    public static AcceptanceDomainException InvalidStateTransition(AcceptanceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class AcceptanceDomainException : Exception
{
    public AcceptanceDomainException(string message) : base(message) { }
}
