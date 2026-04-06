using Whycespace.Shared.Contracts.Domain;

namespace Whycespace.Shared.Contracts.Domain.Constitutional.Policy;

public interface IPolicyRegistryDomainService
{
    Task<DomainOperationResult> CreatePolicyAsync(DomainExecutionContext context, Guid id, string name, string domain, Guid authorId);
    Task<DomainOperationResult> CreatePolicyVersionAsync(DomainExecutionContext context, Guid id, Guid policyId, string rules);
    Task<DomainOperationResult> CreatePolicyScopeAsync(DomainExecutionContext context, Guid id, string cluster, string domain, string contextPath);
    Task<DomainOperationResult> ActivatePolicyVersionAsync(DomainExecutionContext context, Guid versionId);
    Task<DomainOperationResult> LockPolicyAsync(DomainExecutionContext context, Guid policyId);
    Task<bool> CheckScopeAppliesAsync(DomainExecutionContext context, Guid scopeId, string cluster, string domain);
    Task<bool> CheckVersionEffectiveAsync(DomainExecutionContext context, Guid versionId, DateTimeOffset asOf);
}
