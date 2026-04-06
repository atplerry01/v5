using Whycespace.Shared.Primitives.Money;

namespace Whycespace.Shared.Contracts.Domain.Economic.Ledger;

public interface ILedgerDomainService
{
    Task<DomainOperationResult> RecordDoubleEntryAsync(DomainExecutionContext context, string ledgerId, string entryId, string accountCode, string accountName, decimal debitAmount, decimal creditAmount, string currencyCode);
    Task<DomainOperationResult> CreateSettlementAsync(DomainExecutionContext context, string id, string payeeIdentityId, Money amount);
    Task<DomainOperationResult> ExecuteSettlementAsync(DomainExecutionContext context, string settlementId, Guid transactionId, IReadOnlyList<LedgerEntryDto> ledgerEntries, Money amount);
    Task<DomainOperationResult> CreateTreasuryAsync(DomainExecutionContext context, string id);
}

public sealed record LedgerEntryDto(Guid AccountId, decimal Amount, string EntryType, Guid TransactionId);
