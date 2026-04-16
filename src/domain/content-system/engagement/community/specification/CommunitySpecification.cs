using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public sealed class CommunitySpecification : Specification<CommunityStatus>
{
    public override bool IsSatisfiedBy(CommunityStatus entity) => entity == CommunityStatus.Active;

    public void EnsureMutable(CommunityStatus status)
    {
        if (status == CommunityStatus.Archived) throw CommunityErrors.CannotMutateArchived();
    }
}
