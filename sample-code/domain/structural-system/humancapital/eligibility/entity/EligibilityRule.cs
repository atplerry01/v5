namespace Whycespace.Domain.StructuralSystem.HumanCapital.Eligibility;

public sealed class EligibilityRule
{
    public Guid Id { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
