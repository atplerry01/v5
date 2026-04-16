using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed class AssignmentSpecification : Specification<AssignmentStatus>
{
    public override bool IsSatisfiedBy(AssignmentStatus entity) => entity == AssignmentStatus.Published;

    public void EnsurePublishable(AssignmentStatus status)
    {
        if (status != AssignmentStatus.Draft) throw AssignmentErrors.CannotPublishFromStatus(status);
    }

    public void EnsurePublished(AssignmentStatus status)
    {
        if (status != AssignmentStatus.Published) throw AssignmentErrors.NotPublished();
    }
}
