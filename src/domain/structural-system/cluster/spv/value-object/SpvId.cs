using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public readonly record struct SpvId
{
    public Guid Value { get; }

    public SpvId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SpvId cannot be empty.");
        Value = value;
    }
}
