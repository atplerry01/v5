namespace Whycespace.Domain.StructuralSystem.HumanCapital.Performance;

public sealed record Score(double Value)
{
    public bool IsPassable => Value >= 50.0;
}
