using Whycespace.Platform.Api.Core.Contracts.Economic;

namespace Whycespace.Platform.Api.Core.Services.Economic;

/// <summary>
/// Read-only economic query service.
/// All data sourced from CQRS projections (read models) via ProjectionAdapter.
///
/// MUST NOT:
/// - Call engines or domain services
/// - Perform calculations or aggregation logic
/// - Modify any state
/// - Access aggregates or event stores
///
/// Platform surfaces projection data — nothing more.
/// </summary>
public interface IEconomicQueryService
{
    Task<WalletView?> GetWalletAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WalletView>> GetWalletsByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LedgerEntryView>> GetLedgerAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<SettlementView?> GetSettlementAsync(Guid settlementId, CancellationToken cancellationToken = default);
    Task<RevenueView?> GetRevenueAsync(Guid revenueId, CancellationToken cancellationToken = default);
    Task<DistributionView?> GetDistributionAsync(Guid distributionId, CancellationToken cancellationToken = default);
}
