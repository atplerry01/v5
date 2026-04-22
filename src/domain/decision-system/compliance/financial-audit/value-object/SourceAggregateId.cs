using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Compliance.Audit;

public readonly record struct SourceAggregateId
{
    public Guid Value { get; }

    public SourceAggregateId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SourceAggregateId cannot be empty.");
        Value = value;
    }
}