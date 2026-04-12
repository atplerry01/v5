using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public readonly record struct SourceId
{
    public Guid Value { get; }

    public SourceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SourceId cannot be empty.");
        Value = value;
    }
}
