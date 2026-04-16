using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public sealed class QuizSpecification : Specification<QuizStatus>
{
    public override bool IsSatisfiedBy(QuizStatus entity) =>
        entity == QuizStatus.Draft || entity == QuizStatus.Published;

    public void EnsureDraftMutable(QuizStatus status)
    {
        if (status == QuizStatus.Published) throw QuizErrors.CannotMutatePublished();
        if (status == QuizStatus.Archived) throw QuizErrors.AlreadyArchived();
    }
}
