using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public readonly record struct SpvRef
{
    public Guid Value { get; }

    public SpvRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SpvRef cannot be empty.");
        Value = value;
    }
}
