namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public static class StatementErrors
{
    public static StatementDomainException MissingId()
        => new("StatementId is required and must not be empty.");

    public static StatementDomainException EmptyStatement()
        => new("Statement must contain at least one statement line before issuing.");

    public static StatementDomainException AlreadyIssued(StatementId id)
        => new($"Statement '{id.Value}' has already been issued.");

    public static StatementDomainException AlreadyClosed(StatementId id)
        => new($"Statement '{id.Value}' has already been closed.");

    public static StatementDomainException InvalidStateTransition(StatementStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class StatementDomainException : Exception
{
    public StatementDomainException(string message) : base(message) { }
}
