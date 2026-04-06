using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Economic;

namespace Whycespace.Platform.Api.Core.Services.Economic;

/// <summary>
/// Projection-backed economic query service.
/// Delegates all queries to ProjectionAdapter → IProjectionQuerySource.
/// Pure read-only mapping — no business logic, no calculations, no state mutation.
///
/// Projection names follow the convention: "economic.{entity}" or "economic.{entity}.list".
/// </summary>
public sealed class EconomicQueryService : IEconomicQueryService
{
    private readonly ProjectionAdapter _projections;

    public EconomicQueryService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<WalletView?> GetWalletAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<WalletView>(
            "economic.wallet",
            new Dictionary<string, object> { ["walletId"] = walletId },
            cancellationToken: cancellationToken);

        return ExtractData<WalletView>(response);
    }

    public async Task<IReadOnlyList<WalletView>> GetWalletsByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryListAsync<WalletView>(
            "economic.wallet.by-owner",
            new Dictionary<string, object> { ["ownerId"] = ownerId },
            cancellationToken: cancellationToken);

        return ExtractList<WalletView>(response);
    }

    public async Task<IReadOnlyList<LedgerEntryView>> GetLedgerAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryListAsync<LedgerEntryView>(
            "economic.ledger.by-account",
            new Dictionary<string, object> { ["accountId"] = accountId },
            cancellationToken: cancellationToken);

        return ExtractList<LedgerEntryView>(response);
    }

    public async Task<SettlementView?> GetSettlementAsync(Guid settlementId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<SettlementView>(
            "economic.settlement",
            new Dictionary<string, object> { ["settlementId"] = settlementId },
            cancellationToken: cancellationToken);

        return ExtractData<SettlementView>(response);
    }

    public async Task<RevenueView?> GetRevenueAsync(Guid revenueId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<RevenueView>(
            "economic.revenue",
            new Dictionary<string, object> { ["revenueId"] = revenueId },
            cancellationToken: cancellationToken);

        return ExtractData<RevenueView>(response);
    }

    public async Task<DistributionView?> GetDistributionAsync(Guid distributionId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<DistributionView>(
            "economic.distribution",
            new Dictionary<string, object> { ["distributionId"] = distributionId },
            cancellationToken: cancellationToken);

        return ExtractData<DistributionView>(response);
    }

    private static T? ExtractData<T>(Whycespace.Platform.Middleware.ApiResponse response) where T : class
    {
        if (response.StatusCode is < 200 or >= 300)
            return null;

        return response.Data as T;
    }

    private static IReadOnlyList<T> ExtractList<T>(Whycespace.Platform.Middleware.ApiResponse response)
    {
        if (response.StatusCode is < 200 or >= 300)
            return [];

        return response.Data as IReadOnlyList<T> ?? [];
    }
}
