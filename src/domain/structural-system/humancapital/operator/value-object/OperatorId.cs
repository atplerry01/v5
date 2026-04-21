using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Operator;

public readonly record struct OperatorId
{
    public Guid Value { get; }

    public OperatorId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "OperatorId cannot be empty.");
        Value = value;
    }
}
