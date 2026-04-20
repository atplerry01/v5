using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public sealed class CanModifySubtitleSpecification : Specification<SubtitleAggregate>
{
    public override bool IsSatisfiedBy(SubtitleAggregate entity)
        => entity.Status == SubtitleStatus.Draft || entity.Status == SubtitleStatus.Active;
}
