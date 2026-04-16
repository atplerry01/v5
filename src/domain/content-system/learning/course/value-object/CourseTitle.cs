using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Course;

public sealed record CourseTitle : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }
    private CourseTitle(string v) => Value = v;
    public static CourseTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw CourseErrors.InvalidTitle();
        var t = value.Trim();
        if (t.Length > MaxLength) throw CourseErrors.TitleTooLong(MaxLength);
        return new CourseTitle(t);
    }
    public override string ToString() => Value;
}
