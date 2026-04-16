using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public static class ReactionErrors
{
    public static DomainException InvalidActor() => new("Reaction actor reference must be non-empty.");
    public static DomainException InvalidTargetRef() => new("Reaction target reference must be non-empty.");
    public static DomainException AlreadyRemoved() => new("Reaction has been removed.");
    public static DomainException SameKind() => new("Reaction already has this kind.");
    public static DomainInvariantViolationException ActorMissing() =>
        new("Invariant violated: reaction must have an actor.");
    public static DomainInvariantViolationException TargetMissing() =>
        new("Invariant violated: reaction must have a target.");
}
