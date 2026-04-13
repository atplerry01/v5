namespace Whycespace.Shared.Contracts.Policy;

/// <summary>
/// Contract for a policy rule evaluator.
/// Custom policy rules implement this interface.
/// Must be deterministic: same inputs always produce same result.
/// </summary>
public interface IPolicyRule
{
    string RuleId { get; }
    string RuleName { get; }
    bool Evaluate(string identityId, string[] roles, int trustScore, string commandType);
}
