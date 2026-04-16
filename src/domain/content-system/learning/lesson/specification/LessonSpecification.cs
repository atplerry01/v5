using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed class LessonSpecification : Specification<LessonStatus>
{
    public override bool IsSatisfiedBy(LessonStatus entity) =>
        entity == LessonStatus.Draft || entity == LessonStatus.Published;

    public void EnsureMutable(LessonStatus status)
    {
        if (status == LessonStatus.Archived) throw LessonErrors.CannotMutateArchived();
    }
}
