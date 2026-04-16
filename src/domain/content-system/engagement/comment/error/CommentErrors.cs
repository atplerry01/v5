using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public static class CommentErrors
{
    public static DomainException InvalidBody() => new("Comment body must be non-empty.");
    public static DomainException BodyTooLong(int max) => new($"Comment body exceeds {max} characters.");
    public static DomainException InvalidAuthor() => new("Comment author reference must be non-empty.");
    public static DomainException InvalidTargetRef() => new("Comment target reference must be non-empty.");
    public static DomainException InvalidMention() => new("Comment mention is invalid.");
    public static DomainException AlreadyRedacted() => new("Comment has been redacted.");
    public static DomainException CannotMutateRedacted() => new("Redacted comments cannot be modified.");
    public static DomainInvariantViolationException AuthorMissing() =>
        new("Invariant violated: comment must have an author.");
    public static DomainInvariantViolationException TargetMissing() =>
        new("Invariant violated: comment must have a target reference.");
}
