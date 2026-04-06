using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Intelligence.Governance;

public interface IGovernanceAssistDomainService
{
    Task<DomainOperationResult> GenerateRecommendationAsync(DomainExecutionContext context, Guid id, string area, string proposalType, IReadOnlyDictionary<string, object> inputs);
    Task<DomainOperationResult> OptimizeAsync(DomainExecutionContext context, Guid id, string area, IReadOnlyList<object> recommendations);
}
