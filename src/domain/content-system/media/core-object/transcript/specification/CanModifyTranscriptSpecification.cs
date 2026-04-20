using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

public sealed class CanModifyTranscriptSpecification : Specification<TranscriptAggregate>
{
    public override bool IsSatisfiedBy(TranscriptAggregate entity)
        => entity.Status == TranscriptStatus.Draft || entity.Status == TranscriptStatus.Active;
}
