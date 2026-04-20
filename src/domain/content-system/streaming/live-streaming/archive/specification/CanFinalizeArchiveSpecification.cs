using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public sealed class CanFinalizeArchiveSpecification : Specification<ArchiveAggregate>
{
    public override bool IsSatisfiedBy(ArchiveAggregate entity)
        => entity.Status == ArchiveStatus.Completed;
}
