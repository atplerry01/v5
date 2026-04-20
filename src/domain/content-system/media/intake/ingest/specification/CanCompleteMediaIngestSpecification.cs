using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed class CanCompleteMediaIngestSpecification : Specification<MediaIngestAggregate>
{
    public override bool IsSatisfiedBy(MediaIngestAggregate entity)
        => entity.Status == MediaIngestStatus.Processing;
}
