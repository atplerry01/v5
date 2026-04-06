namespace Whycespace.Shared.Contracts.Domain.Economic.Ledger;

public interface IObligationDomainService
{
    Task<DomainOperationResult> CreateAsync(DomainExecutionContext context, string id, string debtorId, string creditorId, decimal amount, string currencyCode);
    Task<DomainOperationResult> ActivateAsync(DomainExecutionContext context, string obligationId);
    Task<DomainOperationResult> SettleAsync(DomainExecutionContext context, string obligationId);
    Task<DomainOperationResult> DefaultAsync(DomainExecutionContext context, string obligationId, string reason);
}
