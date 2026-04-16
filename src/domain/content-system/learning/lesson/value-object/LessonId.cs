namespace Whycespace.Domain.ContentSystem.Learning.Lesson;

public readonly record struct LessonId(Guid Value)
{
    public static LessonId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
