using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Quiz;

public static class QuizErrors
{
    public static DomainException InvalidQuestion() => new("Question must have an id, a non-empty prompt, and positive points.");
    public static DomainException InvalidScore() => new("Quiz score must be within [0, total] with positive total.");
    public static DomainException InvalidCourseRef() => new("Quiz course reference must be non-empty.");
    public static DomainException DuplicateQuestion(Guid id) => new($"Question '{id}' already added.");
    public static DomainException CannotPublishEmpty() => new("Cannot publish a quiz with no questions.");
    public static DomainException AlreadyPublished() => new("Quiz is already published.");
    public static DomainException AlreadyArchived() => new("Quiz is already archived.");
    public static DomainException CannotMutatePublished() => new("Published quizzes cannot be mutated.");
    public static DomainInvariantViolationException CourseMissing() =>
        new("Invariant violated: quiz must belong to a course.");
}
