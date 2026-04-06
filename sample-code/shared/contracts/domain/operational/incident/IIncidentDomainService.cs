namespace Whycespace.Shared.Contracts.Domain.Operational.Incident;

public interface IIncidentDomainService
{
    Task<DomainOperationResult> CreateAsync(
        DomainExecutionContext context,
        string id,
        string title,
        string description,
        string type,
        string severity,
        string source,
        string? referenceId,
        string? correlationId);
}
