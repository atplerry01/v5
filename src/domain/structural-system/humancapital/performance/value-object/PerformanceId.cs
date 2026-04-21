using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Performance;

public readonly record struct PerformanceId
{
    public Guid Value { get; }

    public PerformanceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PerformanceId cannot be empty.");
        Value = value;
    }
}
