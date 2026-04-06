using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

/// <summary>
/// Topic: whyce.economic.settlement.completed
/// Domain uses Money; event uses decimal + currency for serialization.
/// Normalized: single Amount + CurrencyCode. No duplicate numeric fields.
/// </summary>
public sealed record SettlementCompletedEvent(
    Guid SettlementId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
