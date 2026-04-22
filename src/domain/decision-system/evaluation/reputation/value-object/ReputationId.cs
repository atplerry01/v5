using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.DecisionSystem.Evaluation.Reputation;

public readonly record struct ReputationId
{
    public Guid Value { get; }

    public ReputationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReputationId cannot be empty.");
        Value = value;
    }
}
