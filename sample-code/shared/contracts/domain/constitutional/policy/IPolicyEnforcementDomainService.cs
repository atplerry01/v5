using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Constitutional.Policy;

public interface IPolicyEnforcementDomainService
{
    Task<DomainOperationResult> CreateEnforcementActionAsync(DomainExecutionContext context, Guid id, string actionType, string targetId, string reason);
}
