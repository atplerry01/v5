namespace Whycespace.Domain.BusinessSystem.Inventory.Batch;

public static class BatchErrors
{
    public static BatchDomainException MissingId()
        => new("BatchId is required and must not be empty.");

    public static BatchDomainException AlreadyClosed(BatchId id)
        => new($"Batch '{id.Value}' has already been closed and cannot be modified.");

    public static BatchDomainException InvalidStateTransition(BatchStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class BatchDomainException : Exception
{
    public BatchDomainException(string message) : base(message) { }
}
