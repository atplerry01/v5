using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public sealed class CanStartStreamingSpecification : Specification<IngestSessionAggregate>
{
    public override bool IsSatisfiedBy(IngestSessionAggregate entity)
        => entity.Status == IngestSessionStatus.Authenticated;
}
