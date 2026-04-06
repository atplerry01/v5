using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaStepId
{
    public Guid Value { get; }

    public SagaStepId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SagaStepId cannot be empty.", nameof(value));
        Value = value;
    }

    public static SagaStepId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}
