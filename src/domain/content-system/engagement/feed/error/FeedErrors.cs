using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public static class FeedErrors
{
    public static DomainException InvalidItem() => new("Feed item reference must be non-empty.");
    public static DomainException InvalidRank() => new("Feed item rank must be non-negative.");
    public static DomainException InvalidOwner() => new("Feed owner reference must be non-empty.");
    public static DomainException ItemAlreadyPresent(string ref_) => new($"Item '{ref_}' already present.");
    public static DomainException ItemNotPresent(string ref_) => new($"Item '{ref_}' not present.");
    public static DomainInvariantViolationException OwnerMissing() =>
        new("Invariant violated: feed must have an owner.");
}
