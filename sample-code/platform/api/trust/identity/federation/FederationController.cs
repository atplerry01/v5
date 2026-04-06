using Whycespace.Shared.Primitives.Id;
using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Trust.Identity.Federation;

public sealed class FederationController
{
    private const string CommandPrefix = "trust.identity.federation";
    private const string IssuerPrefix = "federation.issuer";
    private const string ProjectionName = "identity-federation";
    private readonly DownstreamAdapter _downstream;
    private readonly ProjectionAdapter _projections;

    public FederationController(DownstreamAdapter downstream, ProjectionAdapter projections)
    { _downstream = downstream; _projections = projections; }

    public Task<ApiResponse> RegisterIssuerAsync(RegisterIssuerDto dto, ApiRequest context)
    {
        var issuerId = DeterministicIdHelper.FromSeed($"{context.RequestId}:{CommandPrefix}").ToString();
        return _downstream.SendCommandAsync($"{IssuerPrefix}.register",
            new { IssuerId = issuerId, dto.Name, dto.IssuerType, dto.InitialTrust },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: issuerId);
    }

    public async Task<ApiResponse> GetFederationLinksAsync(string identityId, ApiRequest context)
    {
        var result = await _projections.QueryAsync<FederationLinksResponse>(
            ProjectionName, new Dictionary<string, object> { ["type"] = "links", ["aggregateId"] = identityId }, context.TraceId);
        if (result.Data is null) return ApiResponse.NotFound($"No federation data for {identityId}.", context.TraceId);
        return result;
    }
}
