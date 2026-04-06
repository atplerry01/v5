using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Constitutional.Policy;

public interface IPolicyEvaluationDomainService
{
    Task<PolicyEvaluationResult> EvaluateConstraintAsync(DomainExecutionContext context, string expression, IReadOnlyDictionary<string, object> facts);
}

public sealed record PolicyEvaluationResult(bool Passed, string? Reason);
