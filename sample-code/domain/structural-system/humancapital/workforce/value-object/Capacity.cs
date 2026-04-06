namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed record Capacity(int Value)
{
    public static readonly Capacity Full = new(0);
}
