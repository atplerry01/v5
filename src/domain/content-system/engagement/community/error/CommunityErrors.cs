using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public static class CommunityErrors
{
    public static DomainException InvalidName() => new("Community name must be non-empty.");
    public static DomainException InvalidMember() => new("Community member reference must be non-empty.");
    public static DomainException InvalidOwner() => new("Community owner reference must be non-empty.");
    public static DomainException MemberAlreadyLeft() => new("Member has already left the community.");
    public static DomainException MemberAlreadyJoined() => new("Member has already joined the community.");
    public static DomainException MemberNotInCommunity() => new("Member is not in the community.");
    public static DomainException AlreadyArchived() => new("Community is already archived.");
    public static DomainException CannotMutateArchived() => new("Archived communities are immutable.");
    public static DomainInvariantViolationException OwnerRequired() =>
        new("Invariant violated: active community must have at least one Owner.");
}
