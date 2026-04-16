using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public static class DistributionErrors
{
    public static DomainException InvalidChannel() => new("Distribution channel name must be non-empty.");
    public static DomainException InvalidAssetRef() => new("Distribution asset reference must be non-empty.");
    public static DomainException ChannelAlreadyPresent(string name) => new($"Channel '{name}' already attached.");
    public static DomainException ChannelNotPresent(string name) => new($"Channel '{name}' not attached.");
    public static DomainException AlreadyDeactivated() => new("Distribution policy already deactivated.");
    public static DomainException CannotMutateDeactivated() => new("Deactivated distribution policy is immutable.");
    public static DomainInvariantViolationException AssetMissing() =>
        new("Invariant violated: distribution policy must reference an asset.");
}
