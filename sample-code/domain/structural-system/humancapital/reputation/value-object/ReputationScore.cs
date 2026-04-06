namespace Whycespace.Domain.StructuralSystem.HumanCapital.Reputation;

public sealed record ReputationScore(double Value)
{
    public bool IsPositive => Value >= 0;
}
