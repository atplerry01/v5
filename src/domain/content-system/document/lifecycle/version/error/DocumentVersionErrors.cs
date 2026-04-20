using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public static class DocumentVersionErrors
{
    public static DomainException VersionNotDraft()
        => new("Only draft versions can be activated.");

    public static DomainException VersionAlreadyActive()
        => new("Version is already active.");

    public static DomainException VersionAlreadySuperseded()
        => new("Version is already superseded.");

    public static DomainException VersionAlreadyWithdrawn()
        => new("Version is already withdrawn.");

    public static DomainException CannotSupersedeWithSelf()
        => new("Successor version id must differ from current version id.");

    public static DomainException CannotSupersedeNonActive()
        => new("Only active versions can be superseded.");

    public static DomainException InvalidWithdrawalReason()
        => new("Withdrawal reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedVersion()
        => new("Document version must reference an owning document.");
}
