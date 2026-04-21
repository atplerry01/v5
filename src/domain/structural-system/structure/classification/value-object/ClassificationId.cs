using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public readonly record struct ClassificationId
{
    public Guid Value { get; }

    public ClassificationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClassificationId cannot be empty.");
        Value = value;
    }
}
