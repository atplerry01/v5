namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

public static class ValidityErrors
{
    public static ValidityDomainException MissingId()
        => new("ValidityId is required and must not be empty.");

    public static ValidityDomainException AlreadyInvalid(ValidityId id)
        => new($"Validity '{id.Value}' is already invalid.");

    public static ValidityDomainException AlreadyExpired(ValidityId id)
        => new($"Validity '{id.Value}' has already expired.");

    public static ValidityDomainException InvalidStateTransition(ValidityStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ValidityDomainException : Exception
{
    public ValidityDomainException(string message) : base(message) { }
}
