namespace Whycespace.Domain.ContentSystem.Learning.Course;

public readonly record struct CourseId(Guid Value)
{
    public static CourseId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
