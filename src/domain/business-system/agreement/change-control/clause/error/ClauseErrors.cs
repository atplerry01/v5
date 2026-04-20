namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public static class ClauseErrors
{
    public static ClauseDomainException MissingId()
        => new("ClauseId is required and must not be empty.");

    public static ClauseDomainException InvalidClauseType()
        => new("ClauseType must be a valid defined value.");

    public static ClauseDomainException AlreadySuperseded(ClauseId id)
        => new($"Clause '{id.Value}' has already been superseded.");

    public static ClauseDomainException InvalidStateTransition(ClauseStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class ClauseDomainException : Exception
{
    public ClauseDomainException(string message) : base(message) { }
}
