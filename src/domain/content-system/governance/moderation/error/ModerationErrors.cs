using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public static class ModerationErrors
{
    public static DomainException InvalidTargetRef() => new("Moderation target reference must be non-empty.");
    public static DomainException InvalidReporter() => new("Moderation reporter reference must be non-empty.");
    public static DomainException InvalidEvidence() => new("Evidence record is invalid.");
    public static DomainException InvalidDecision() => new("Decision must be one of Allowed/Redacted/Removed/Banned.");
    public static DomainException AlreadyClosed() => new("Moderation case already closed.");
    public static DomainException NotDecided() => new("Moderation case has not been decided yet.");
    public static DomainException CannotReopenClosed() => new("Closed moderation cases cannot be reopened.");
    public static DomainInvariantViolationException TargetMissing() =>
        new("Invariant violated: moderation case must have a target reference.");
}
