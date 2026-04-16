namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public readonly record struct AssignmentId(Guid Value)
{
    public static AssignmentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
