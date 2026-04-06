namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed record Availability(string Value)
{
    public static readonly Availability Available = new("available");
    public static readonly Availability Unavailable = new("unavailable");
}
