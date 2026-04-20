using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public static class ChannelErrors
{
    public static DomainException ChannelArchived()
        => new("Cannot mutate an archived channel.");

    public static DomainException ChannelAlreadyEnabled()
        => new("Channel is already enabled.");

    public static DomainException ChannelAlreadyDisabled()
        => new("Channel is already disabled.");

    public static DomainException ChannelAlreadyArchived()
        => new("Channel is already archived.");

    public static DomainException InvalidDisableReason()
        => new("Disable reason cannot be empty.");

    public static DomainInvariantViolationException OrphanedChannel()
        => new("Channel must reference a parent stream.");
}
