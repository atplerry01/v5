using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public sealed class CourseSpecification : Specification<CourseStatus>
{
    public override bool IsSatisfiedBy(CourseStatus entity) =>
        entity == CourseStatus.Draft || entity == CourseStatus.Published;

    public void EnsureMutable(CourseStatus status)
    {
        if (status == CourseStatus.Archived) throw CourseErrors.CannotMutateArchived();
    }

    public void EnsurePublishable(CourseStatus status, int moduleCount)
    {
        if (status == CourseStatus.Published) throw CourseErrors.AlreadyPublished();
        if (status == CourseStatus.Archived) throw CourseErrors.CannotMutateArchived();
        if (moduleCount == 0) throw CourseErrors.CannotPublishEmpty();
    }
}
