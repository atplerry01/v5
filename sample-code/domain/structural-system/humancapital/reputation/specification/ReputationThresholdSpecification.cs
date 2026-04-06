namespace Whycespace.Domain.StructuralSystem.HumanCapital.Reputation;

public sealed class ReputationThresholdSpecification
{
    public bool IsSatisfiedBy(ReputationAggregate reputation) => reputation.Score.IsPositive;
}
