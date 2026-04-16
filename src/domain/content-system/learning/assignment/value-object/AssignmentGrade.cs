using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed record AssignmentGrade : ValueObject
{
    public decimal Value { get; }
    public decimal Max { get; }

    private AssignmentGrade(decimal value, decimal max)
    {
        Value = value;
        Max = max;
    }

    public static AssignmentGrade Create(decimal value, decimal max)
    {
        if (max <= 0m) throw AssignmentErrors.InvalidGrade();
        if (value < 0m || value > max) throw AssignmentErrors.InvalidGrade();
        return new AssignmentGrade(value, max);
    }

    public decimal Percentage => Value / Max * 100m;
}
