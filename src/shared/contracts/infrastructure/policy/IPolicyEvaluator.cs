namespace Whycespace.Shared.Contracts.Infrastructure.Policy;

public interface IPolicyEvaluator
{
    Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext);
}

public sealed record PolicyDecision(
    bool IsAllowed,
    string PolicyId,
    string DecisionHash,
    string? DenialReason);

public sealed record PolicyContext(
    Guid CorrelationId,
    string TenantId,
    string ActorId,
    string CommandType,
    string[] Roles,
    string Classification,
    string Context,
    string Domain,
    IReadOnlyDictionary<string, object>? SubjectAttributes = null);
