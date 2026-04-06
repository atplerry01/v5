namespace Whycespace.Shared.Contracts.Domain.Operational.Todo;

public interface ITodoDomainService
{
    Task<DomainOperationResult> CreateAsync(DomainExecutionContext context, Guid todoId, string title, string description, int priority);
    Task<DomainOperationResult> CompleteAsync(DomainExecutionContext context, Guid todoId);
}
