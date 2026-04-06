namespace Whycespace.Domain.StructuralSystem.HumanCapital.Governance;

public sealed class GovernanceRule
{
    public Guid Id { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
