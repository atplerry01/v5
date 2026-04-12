namespace Whycespace.Domain.BusinessSystem.Execution.Completion;

public static class CompletionErrors
{
    public static CompletionDomainException MissingId()
        => new("CompletionId is required and must not be empty.");

    public static CompletionDomainException MissingTargetId()
        => new("CompletionTargetId is required and must not be empty.");

    public static CompletionDomainException InvalidStateTransition(CompletionStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CompletionDomainException AlreadyCompleted(CompletionId id)
        => new($"Completion '{id.Value}' has already been completed.");

    public static CompletionDomainException AlreadyFailed(CompletionId id)
        => new($"Completion '{id.Value}' has already failed.");
}

public sealed class CompletionDomainException : Exception
{
    public CompletionDomainException(string message) : base(message) { }
}
