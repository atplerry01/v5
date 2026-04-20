using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed class CanEnableChannelSpecification : Specification<ChannelAggregate>
{
    public override bool IsSatisfiedBy(ChannelAggregate entity)
        => entity.Status == ChannelStatus.Created || entity.Status == ChannelStatus.Disabled;
}
