using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public static class DocumentRecordErrors
{
    public static DomainException RecordArchived()
        => new("Cannot mutate an archived record.");

    public static DomainException RecordClosed()
        => new("Cannot mutate a closed record.");

    public static DomainException AlreadyLocked()
        => new("Record is already locked.");

    public static DomainException NotLocked()
        => new("Record is not locked.");

    public static DomainException AlreadyClosed()
        => new("Record is already closed.");

    public static DomainException AlreadyArchived()
        => new("Record is already archived.");

    public static DomainException InvalidLockReason()
        => new("Lock reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedRecord()
        => new("Document record must reference an owning document.");
}
