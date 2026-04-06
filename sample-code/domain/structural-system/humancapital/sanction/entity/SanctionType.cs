namespace Whycespace.Domain.StructuralSystem.HumanCapital.Sanction;

public sealed class SanctionType
{
    public Guid Id { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
}
