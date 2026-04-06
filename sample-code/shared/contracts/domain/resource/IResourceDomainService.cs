namespace Whycespace.Shared.Contracts.Domain.Resource;

public interface IResourceDomainService
{
    Task<DomainOperationResult> CreateReservationAsync(DomainExecutionContext context, string id);
    Task<DomainOperationResult> CreateCapacityAsync(DomainExecutionContext context, string id);
}
