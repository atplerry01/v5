using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public static class MediaVersionErrors
{
    public static DomainException VersionNotDraft()
        => new("Only draft media versions can be activated.");

    public static DomainException VersionAlreadyActive()
        => new("Media version is already active.");

    public static DomainException VersionAlreadySuperseded()
        => new("Media version is already superseded.");

    public static DomainException VersionAlreadyWithdrawn()
        => new("Media version is already withdrawn.");

    public static DomainException CannotSupersedeWithSelf()
        => new("Successor media version id must differ from current version id.");

    public static DomainException CannotSupersedeNonActive()
        => new("Only active media versions can be superseded.");

    public static DomainException InvalidWithdrawalReason()
        => new("Withdrawal reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedMediaVersion()
        => new("Media version must reference an owning asset.");
}
