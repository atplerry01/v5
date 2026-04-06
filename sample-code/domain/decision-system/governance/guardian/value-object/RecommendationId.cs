using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

public sealed record RecommendationId
{
    public Guid Value { get; }

    private RecommendationId(Guid value) => Value = value;

    public static RecommendationId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static RecommendationId From(Guid value) =>
        value == Guid.Empty
            ? throw new ArgumentException("RecommendationId must not be empty.", nameof(value))
            : new(value);

    public override string ToString() => Value.ToString();
}
