namespace Whycespace.Shared.Contracts.Domain.Workflow;

public interface IWorkflowDomainService
{
    Task<DomainOperationResult> CreateDefinitionAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> CreateInstanceAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> CreateStepAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> CreateTransitionAsync(DomainExecutionContext context, string id);
}
