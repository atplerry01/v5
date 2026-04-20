using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public static class RetentionErrors
{
    public static DomainException RetentionArchived()
        => new("Cannot mutate an archived retention.");

    public static DomainException AlreadyArchived()
        => new("Retention is already archived.");

    public static DomainException CannotPlaceHoldOnTerminal()
        => new("Cannot place a hold on a released, expired, or destruction-eligible retention.");

    public static DomainException AlreadyHeld()
        => new("Retention is already on hold.");

    public static DomainException NotHeld()
        => new("Retention is not on hold.");

    public static DomainException AlreadyReleased()
        => new("Retention is already released.");

    public static DomainException AlreadyExpired()
        => new("Retention has already expired.");

    public static DomainException AlreadyEligibleForDestruction()
        => new("Retention is already marked eligible for destruction.");

    public static DomainException CannotMarkEligibleUnlessExpired()
        => new("Retention must be expired before being marked eligible for destruction.");

    public static DomainInvariantViolationException OrphanedRetention()
        => new("Retention must reference a valid target.");
}
