using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Compliance.Audit;

public readonly record struct SourceEventId
{
    public Guid Value { get; }

    public SourceEventId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SourceEventId cannot be empty.");
        Value = value;
    }
}