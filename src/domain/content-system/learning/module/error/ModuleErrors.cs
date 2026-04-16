using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public static class ModuleErrors
{
    public static DomainException InvalidTitle() => new("Module title must be non-empty.");
    public static DomainException TitleTooLong(int max) => new($"Module title exceeds {max} characters.");
    public static DomainException InvalidCourseRef() => new("Course reference must be non-empty.");
    public static DomainException InvalidOrder() => new("Module order must be non-negative.");
    public static DomainException AlreadyPublished() => new("Module is already published.");
    public static DomainException AlreadyArchived() => new("Module is already archived.");
    public static DomainException CannotMutateArchived() => new("Archived modules are immutable.");
    public static DomainInvariantViolationException CourseMissing() =>
        new("Invariant violated: module must belong to a course.");
}
