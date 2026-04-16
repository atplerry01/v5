using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public sealed record LessonBody : ValueObject
{
    public const int MaxLength = 50_000;
    public string Value { get; }
    private LessonBody(string v) => Value = v;
    public static LessonBody Create(string v)
    {
        var s = (v ?? string.Empty).Trim();
        if (s.Length == 0) throw LessonErrors.InvalidBody();
        if (s.Length > MaxLength) throw LessonErrors.BodyTooLong(MaxLength);
        return new LessonBody(s);
    }
    public override string ToString() => Value;
}
