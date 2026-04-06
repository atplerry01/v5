namespace Whycespace.Shared.Contracts.Domain.Constitutional;

public interface IPolicyDomainService
{
    Task<DomainOperationResult> CreateEnforcementAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> CreateRuleAsync(DomainExecutionContext context, string id);
}
