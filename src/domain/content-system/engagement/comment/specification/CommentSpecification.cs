using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed class CommentSpecification : Specification<CommentStatus>
{
    public override bool IsSatisfiedBy(CommentStatus entity) =>
        entity == CommentStatus.Posted || entity == CommentStatus.Edited;

    public void EnsureMutable(CommentStatus status)
    {
        if (status == CommentStatus.Redacted) throw CommentErrors.CannotMutateRedacted();
    }
}
