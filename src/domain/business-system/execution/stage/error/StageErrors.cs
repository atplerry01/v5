namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public static class StageErrors
{
    public static StageDomainException MissingId()
        => new("StageId is required and must not be empty.");
    public static StageDomainException MissingContextId()
        => new("StageContextId is required and must not be empty.");
    public static StageDomainException AlreadyStarted()
        => new("Stage has already been started.");
    public static StageDomainException AlreadyCompleted()
        => new("Stage has already been completed.");
    public static StageDomainException InvalidStateTransition(StageStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class StageDomainException : Exception
{
    public StageDomainException(string message) : base(message) { }
}
