namespace Whycespace.Shared.Contracts.Economic.Enforcement.Rule;

public sealed record EnforcementRuleReadModel
{
    public Guid RuleId { get; init; }
    public string RuleCode { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public string RuleCategory { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
