namespace Whyce.Shared.Contracts.Policy;

/// <summary>
/// Contract for external policy rule providers.
/// Allows loading policy rules from configuration, database, or external systems.
/// </summary>
public interface IPolicyRegistry
{
    Task<IReadOnlyList<PolicyRuleDefinition>> GetRulesAsync(string policyName);
    Task<IReadOnlyList<PolicyRuleDefinition>> GetAllRulesAsync();
}

public sealed record PolicyRuleDefinition(
    string RuleId,
    string RuleName,
    string PolicyName,
    int Priority,
    string RuleHash);
