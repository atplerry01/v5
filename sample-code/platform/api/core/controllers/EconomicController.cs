using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Platform.Api.Core.Services.Economic;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus economic interface controller.
/// READ-ONLY access to economic projections (CQRS read models).
///
/// PLATFORM GUARDS:
/// - GET only — no POST/PUT/PATCH/DELETE
/// - No mutation, no commands, no state changes
/// - All data sourced from projections via ProjectionAdapter
/// - Identity required (WhyceId must be present)
/// - No direct engine or domain aggregate access
///
/// Endpoints:
///   GET /api/economic/wallet/{id}
///   GET /api/economic/wallet/owner/{ownerId}
///   GET /api/economic/ledger/{accountId}
///   GET /api/economic/settlement/{id}
///   GET /api/economic/revenue/{id}
///   GET /api/economic/distribution/{id}
/// </summary>
public sealed class EconomicController
{
    private readonly IEconomicQueryService _queryService;

    public EconomicController(IEconomicQueryService queryService)
    {
        _queryService = queryService;
    }

    /// <summary>
    /// Handles all economic query requests.
    /// Enforces GET-only, identity-required, projection-based read access.
    /// </summary>
    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: Read-only enforcement — reject non-GET methods
        if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "Economic interface is read-only — only GET requests are permitted", request.TraceId);

        // Guard: Identity required
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;

        // Route to the correct query based on endpoint
        return await RouteQuery(request.Endpoint, correlationId, request.TraceId, cancellationToken);
    }

    private async Task<ApiResponse> RouteQuery(
        string endpoint,
        string correlationId,
        string? traceId,
        CancellationToken cancellationToken)
    {
        // Parse: /api/economic/{resource}/{id}
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Expected: ["api", "economic", resource, id] or ["api", "economic", resource, sub, id]
        if (segments.Length < 4)
            return ApiResponse.BadRequest("Invalid economic endpoint format", traceId);

        var resource = segments[2].ToLowerInvariant();

        return resource switch
        {
            "wallet" => await HandleWalletQuery(segments, correlationId, traceId, cancellationToken),
            "ledger" => await HandleLedgerQuery(segments, correlationId, traceId, cancellationToken),
            "settlement" => await HandleSettlementQuery(segments, correlationId, traceId, cancellationToken),
            "revenue" => await HandleRevenueQuery(segments, correlationId, traceId, cancellationToken),
            "distribution" => await HandleDistributionQuery(segments, correlationId, traceId, cancellationToken),
            _ => ApiResponse.NotFound($"Unknown economic resource: {resource}", traceId)
        };
    }

    private async Task<ApiResponse> HandleWalletQuery(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        // GET /api/economic/wallet/owner/{ownerId}
        if (segments.Length >= 5 && string.Equals(segments[3], "owner", StringComparison.OrdinalIgnoreCase))
        {
            if (!Guid.TryParse(segments[4], out var ownerId))
                return ApiResponse.BadRequest("Invalid owner ID format", traceId);

            var wallets = await _queryService.GetWalletsByOwnerAsync(ownerId, ct);
            return ApiResponse.Ok(
                WhyceResponse.Ok(wallets, correlationId, traceId), traceId);
        }

        // GET /api/economic/wallet/{id}
        if (!Guid.TryParse(segments[3], out var walletId))
            return ApiResponse.BadRequest("Invalid wallet ID format", traceId);

        var wallet = await _queryService.GetWalletAsync(walletId, ct);
        return wallet is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(wallet, correlationId, traceId), traceId)
            : ApiResponse.NotFound(
                WhyceResponse.NotFound("wallet", correlationId, traceId).Result?.ToString() ?? "Wallet not found",
                traceId);
    }

    private async Task<ApiResponse> HandleLedgerQuery(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var accountId))
            return ApiResponse.BadRequest("Invalid account ID format", traceId);

        var entries = await _queryService.GetLedgerAsync(accountId, ct);
        return ApiResponse.Ok(
            WhyceResponse.Ok(entries, correlationId, traceId), traceId);
    }

    private async Task<ApiResponse> HandleSettlementQuery(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var settlementId))
            return ApiResponse.BadRequest("Invalid settlement ID format", traceId);

        var settlement = await _queryService.GetSettlementAsync(settlementId, ct);
        return settlement is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(settlement, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Settlement not found", traceId);
    }

    private async Task<ApiResponse> HandleRevenueQuery(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var revenueId))
            return ApiResponse.BadRequest("Invalid revenue ID format", traceId);

        var revenue = await _queryService.GetRevenueAsync(revenueId, ct);
        return revenue is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(revenue, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Revenue not found", traceId);
    }

    private async Task<ApiResponse> HandleDistributionQuery(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var distributionId))
            return ApiResponse.BadRequest("Invalid distribution ID format", traceId);

        var distribution = await _queryService.GetDistributionAsync(distributionId, ct);
        return distribution is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(distribution, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Distribution not found", traceId);
    }
}
