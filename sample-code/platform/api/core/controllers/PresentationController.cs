using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Presentation;
using Whycespace.Platform.Api.Core.Services.Economic;
using Whycespace.Platform.Api.Core.Services.Governance;
using Whycespace.Platform.Api.Core.Services.Graph;
using Whycespace.Platform.Api.Core.Services.Presentation;
using Whycespace.Platform.Api.Core.Services.Workflow;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus UI/UX presentation layer controller.
/// Transforms domain view models into UI-ready presentation structures.
///
/// PLATFORM GUARDS:
/// - GET only — no mutation
/// - Pure transformation — no business logic, no external calls
/// - Identity required (WhyceId)
/// - All data sourced from existing query services (which read from projections)
///
/// Endpoints:
///   GET /api/ui/workflow/{workflowId}
///   GET /api/ui/economic/{walletId}
///   GET /api/ui/economic/{walletId}/ledger
///   GET /api/ui/governance/{decisionId}
///   GET /api/ui/graph/{spvId}
/// </summary>
public sealed class PresentationController
{
    private readonly IUiMapper _mapper;
    private readonly IWorkflowQueryService _workflowQuery;
    private readonly IEconomicQueryService _economicQuery;
    private readonly IGovernanceQueryService _governanceQuery;
    private readonly ISpvGraphQueryService _graphQuery;

    public PresentationController(
        IUiMapper mapper,
        IWorkflowQueryService workflowQuery,
        IEconomicQueryService economicQuery,
        IGovernanceQueryService governanceQuery,
        ISpvGraphQueryService graphQuery)
    {
        _mapper = mapper;
        _workflowQuery = workflowQuery;
        _economicQuery = economicQuery;
        _governanceQuery = governanceQuery;
        _graphQuery = graphQuery;
    }

    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: Read-only
        if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "Presentation interface is read-only — only GET requests are permitted", request.TraceId);

        // Guard: Identity required
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;
        var traceId = request.TraceId ?? request.RequestId;

        return await RouteQuery(request.Endpoint, correlationId, traceId, cancellationToken);
    }

    private async Task<ApiResponse> RouteQuery(
        string endpoint,
        string correlationId,
        string? traceId,
        CancellationToken cancellationToken)
    {
        // Parse: /api/ui/{domain}/{id}[/{sub}]
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 4)
            return ApiResponse.BadRequest("Invalid presentation endpoint — expected /api/ui/{domain}/{id}", traceId);

        var domain = segments[2].ToLowerInvariant();

        return domain switch
        {
            "workflow" => await HandleWorkflowUi(segments, correlationId, traceId, cancellationToken),
            "economic" => await HandleEconomicUi(segments, correlationId, traceId, cancellationToken),
            "governance" => await HandleGovernanceUi(segments, correlationId, traceId, cancellationToken),
            "graph" => await HandleGraphUi(segments, correlationId, traceId, cancellationToken),
            _ => ApiResponse.NotFound($"Unknown UI domain: {domain}", traceId)
        };
    }

    private async Task<ApiResponse> HandleWorkflowUi(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var workflowId))
            return ApiResponse.BadRequest("Invalid workflow ID format", traceId);

        var workflow = await _workflowQuery.GetWorkflowAsync(workflowId, ct);
        if (workflow is null)
            return ApiResponse.NotFound("Workflow not found", traceId);

        var card = _mapper.MapWorkflow(workflow);

        var timeline = await _workflowQuery.GetTimelineAsync(workflowId, ct);
        var timelineItems = timeline is not null
            ? _mapper.MapWorkflowTimeline(timeline)
            : (IReadOnlyList<UiTimelineItem>)[];

        var dashboard = new UiDashboard
        {
            Cards = [card],
            Timeline = timelineItems
        };

        return ApiResponse.Ok(WhyceResponse.Ok(dashboard, correlationId, traceId), traceId);
    }

    private async Task<ApiResponse> HandleEconomicUi(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var walletId))
            return ApiResponse.BadRequest("Invalid wallet ID format", traceId);

        // /api/ui/economic/{walletId}/ledger
        if (segments.Length >= 5 && string.Equals(segments[4], "ledger", StringComparison.OrdinalIgnoreCase))
        {
            var entries = await _economicQuery.GetLedgerAsync(walletId, ct);
            var items = _mapper.MapLedgerEntries(entries);
            var dashboard = new UiDashboard { Items = items };
            return ApiResponse.Ok(WhyceResponse.Ok(dashboard, correlationId, traceId), traceId);
        }

        // /api/ui/economic/{walletId}
        var wallet = await _economicQuery.GetWalletAsync(walletId, ct);
        if (wallet is null)
            return ApiResponse.NotFound("Wallet not found", traceId);

        var card = _mapper.MapWallet(wallet);
        var dashboard2 = new UiDashboard { Cards = [card] };
        return ApiResponse.Ok(WhyceResponse.Ok(dashboard2, correlationId, traceId), traceId);
    }

    private async Task<ApiResponse> HandleGovernanceUi(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var decisionId))
            return ApiResponse.BadRequest("Invalid decision ID format", traceId);

        var decision = await _governanceQuery.GetDecisionAsync(decisionId, ct);
        if (decision is null)
            return ApiResponse.NotFound("Decision not found", traceId);

        var card = _mapper.MapGovernanceDecision(decision);

        var timeline = await _governanceQuery.GetTimelineAsync(decisionId, ct);
        var timelineItems = timeline is not null
            ? _mapper.MapGovernanceTimeline(timeline)
            : (IReadOnlyList<UiTimelineItem>)[];

        var dashboard = new UiDashboard
        {
            Cards = [card],
            Timeline = timelineItems
        };

        return ApiResponse.Ok(WhyceResponse.Ok(dashboard, correlationId, traceId), traceId);
    }

    private async Task<ApiResponse> HandleGraphUi(
        string[] segments, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(segments[3], out var spvId))
            return ApiResponse.BadRequest("Invalid SPV ID format", traceId);

        var graph = await _graphQuery.GetGraphAsync(spvId, ct);
        if (graph is null)
            return ApiResponse.NotFound("SPV graph not found", traceId);

        var graphData = _mapper.MapSpvGraph(graph);
        var dashboard = new UiDashboard { Graph = graphData };
        return ApiResponse.Ok(WhyceResponse.Ok(dashboard, correlationId, traceId), traceId);
    }
}
