using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public static class LessonErrors
{
    public static DomainException InvalidBody() => new("Lesson body must be non-empty.");
    public static DomainException BodyTooLong(int max) => new($"Lesson body exceeds {max} characters.");
    public static DomainException InvalidModuleRef() => new("Lesson module reference must be non-empty.");
    public static DomainException AlreadyPublished() => new("Lesson is already published.");
    public static DomainException AlreadyArchived() => new("Lesson is already archived.");
    public static DomainException CannotMutateArchived() => new("Archived lessons are immutable.");
    public static DomainInvariantViolationException ModuleMissing() =>
        new("Invariant violated: lesson must belong to a module.");
}
