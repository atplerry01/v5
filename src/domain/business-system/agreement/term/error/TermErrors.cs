namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public static class TermErrors
{
    public static TermDomainException MissingId()
        => new("TermId is required and must not be empty.");

    public static TermDomainException InvalidDuration()
        => new("Term duration must be greater than zero days.");

    public static TermDomainException AlreadyActive(TermId id)
        => new($"Term '{id.Value}' is already active.");

    public static TermDomainException AlreadyExpired(TermId id)
        => new($"Term '{id.Value}' has already expired.");

    public static TermDomainException InvalidStateTransition(TermStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class TermDomainException : Exception
{
    public TermDomainException(string message) : base(message) { }
}
