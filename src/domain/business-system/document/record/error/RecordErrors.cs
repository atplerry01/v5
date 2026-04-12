namespace Whycespace.Domain.BusinessSystem.Document.Record;

public static class RecordErrors
{
    public static RecordDomainException MissingId()
        => new("RecordId is required and must not be empty.");

    public static RecordDomainException AlreadyLocked(RecordId id)
        => new($"Record '{id.Value}' has already been locked.");

    public static RecordDomainException AlreadyArchived(RecordId id)
        => new($"Record '{id.Value}' has already been archived.");

    public static RecordDomainException InvalidStateTransition(RecordStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RecordDomainException CannotModifyLockedRecord()
        => new("Locked records cannot be modified.");
}

public sealed class RecordDomainException : Exception
{
    public RecordDomainException(string message) : base(message) { }
}
