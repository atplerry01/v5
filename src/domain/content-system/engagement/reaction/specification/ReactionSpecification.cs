using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public sealed class ReactionSpecification : Specification<ReactionStatus>
{
    public override bool IsSatisfiedBy(ReactionStatus entity) =>
        entity == ReactionStatus.Added || entity == ReactionStatus.Changed;

    public void EnsureActive(ReactionStatus status)
    {
        if (status == ReactionStatus.Removed) throw ReactionErrors.AlreadyRemoved();
    }
}
