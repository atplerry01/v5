using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public static class ModerationErrors
{
    public static DomainException EmptyFlagReason()
        => new("Flag reason cannot be empty.");

    public static DomainException EmptyRationale()
        => new("Rationale cannot be empty.");

    public static DomainException CannotAssignUnlessFlagged()
        => new("Moderation can only be assigned when in Flagged status.");

    public static DomainException CannotDecideUnlessInReview()
        => new("Moderation can only be decided when in InReview status.");

    public static DomainException CannotOverturnUnlessDecided()
        => new("Moderation can only be overturned when in Decided status.");

    public static DomainInvariantViolationException MissingModerationId()
        => new("Moderation requires a valid ModerationId.");

    public static DomainInvariantViolationException MissingTargetRef()
        => new("Moderation requires a valid TargetRef.");
}
