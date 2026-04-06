using Whycespace.Shared.Primitives.Money;

namespace Whycespace.Shared.Contracts.Domain.Structural;

public interface ICrossSpvDomainService
{
    Task<DomainOperationResult> CreateCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string rootSpvId,
        IReadOnlyList<SpvLegDto> legs);

    Task<DomainOperationResult> PrepareCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string transactionId);

    Task<DomainOperationResult> CommitCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string transactionId);

    Task<DomainOperationResult> FailCrossSpvTransactionAsync(
        DomainExecutionContext context,
        string id,
        string transactionId,
        string reason);

    Task<DomainOperationResult> SetExecutionStateAsync(
        DomainExecutionContext context,
        string id,
        string state);
}

public sealed record SpvLegDto(
    Guid FromSpvId,
    Guid ToSpvId,
    decimal Amount,
    string CurrencyCode);
