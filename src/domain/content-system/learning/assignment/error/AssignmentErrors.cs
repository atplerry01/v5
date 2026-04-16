using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public static class AssignmentErrors
{
    public static DomainException InvalidTitle() => new("Assignment title must be non-empty.");
    public static DomainException InvalidCourseRef() => new("Assignment course reference must be non-empty.");
    public static DomainException InvalidGrade() => new("Grade must be within [0, max] with positive max.");
    public static DomainException InvalidSubmission() => new("Submission must have an id, learner, and content ref.");
    public static DomainException AlreadyGraded() => new("Submission has already been graded.");
    public static DomainException NotPublished() => new("Assignment is not published.");
    public static DomainException AlreadyClosed() => new("Assignment is already closed.");
    public static DomainException UnknownSubmission(Guid id) => new($"Submission '{id}' not found.");
    public static DomainException CannotPublishFromStatus(AssignmentStatus s) =>
        new($"Assignment cannot be published from {s}.");
    public static DomainInvariantViolationException CourseMissing() =>
        new("Invariant violated: assignment must belong to a course.");
}
