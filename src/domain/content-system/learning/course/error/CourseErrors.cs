using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public static class CourseErrors
{
    public static DomainException InvalidTitle() => new("Course title must be non-empty.");
    public static DomainException TitleTooLong(int max) => new($"Course title exceeds {max} characters.");
    public static DomainException InvalidOwner() => new("Course owner reference must be non-empty.");
    public static DomainException InvalidModuleRef() => new("Module reference must be non-empty.");
    public static DomainException InvalidOrder() => new("Outline order must be non-negative.");
    public static DomainException ModuleAlreadyAttached(string ref_) => new($"Module '{ref_}' already attached.");
    public static DomainException ModuleNotAttached(string ref_) => new($"Module '{ref_}' not attached.");
    public static DomainException CannotPublishEmpty() => new("Cannot publish course with no modules.");
    public static DomainException AlreadyPublished() => new("Course is already published.");
    public static DomainException AlreadyArchived() => new("Course is already archived.");
    public static DomainException CannotMutateArchived() => new("Archived courses are immutable.");
    public static DomainInvariantViolationException OwnerMissing() =>
        new("Invariant violated: course must have an owner.");
}
