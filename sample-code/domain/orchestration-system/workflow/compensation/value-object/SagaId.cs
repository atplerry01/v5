using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed record SagaId
{
    public Guid Value { get; }

    public SagaId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SagaId cannot be empty.", nameof(value));
        Value = value;
    }

    public static SagaId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public override string ToString() => Value.ToString();
}