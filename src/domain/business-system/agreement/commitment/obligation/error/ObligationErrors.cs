namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

public static class ObligationErrors
{
    public static ObligationDomainException MissingId()
        => new("ObligationId is required and must not be empty.");

    public static ObligationDomainException AlreadyFulfilled(ObligationId id)
        => new($"Obligation '{id.Value}' has already been fulfilled.");

    public static ObligationDomainException AlreadyBreached(ObligationId id)
        => new($"Obligation '{id.Value}' has already been breached.");

    public static ObligationDomainException InvalidStateTransition(ObligationStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ObligationDomainException : Exception
{
    public ObligationDomainException(string message) : base(message) { }
}
