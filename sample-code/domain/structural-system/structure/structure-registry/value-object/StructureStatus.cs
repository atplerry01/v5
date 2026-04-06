namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureStatus(string Value)
{
    public static readonly StructureStatus Active = new("ACTIVE");
    public static readonly StructureStatus Inactive = new("INACTIVE");
    public static readonly StructureStatus Suspended = new("SUSPENDED");

    public bool IsTerminal => this == Inactive;

    public override string ToString() => Value;
}
