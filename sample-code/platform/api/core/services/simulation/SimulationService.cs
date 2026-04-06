using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Simulation;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Services.Simulation;

/// <summary>
/// T3I simulation adapter. Delegates simulation to the T3I engine tier
/// via DownstreamAdapter and maps results to platform view models.
///
/// HARD SAFETY RULES:
/// - ZERO simulation logic — pure delegation to T3I
/// - ZERO domain access — no aggregates, no event stores
/// - ZERO state mutation — no writes, no events, no side effects
/// - ZERO runtime execution — no T2E, no workflow dispatch
/// - Deterministic: same input produces same output
///
/// Flow: SimulationRequest -> DownstreamAdapter -> T3I Engine -> EngineResult -> SimulationResultView
/// </summary>
public sealed class SimulationService : ISimulationService
{
    private const string SimulationCommandType = "simulation.t3i.execute";

    private readonly DownstreamAdapter _downstream;
    private readonly IPolicyPreviewService _policyPreview;

    public SimulationService(DownstreamAdapter downstream, IPolicyPreviewService policyPreview)
    {
        _downstream = downstream;
        _policyPreview = policyPreview;
    }

    public async Task<SimulationResultView> SimulateAsync(
        SimulationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Policy pre-check (advisory only)
        var policyPreview = await RunPolicyPreviewAsync(request, cancellationToken);

        // Step 2: Delegate to T3I simulation engine via adapter
        var engineResponse = await _downstream.SendCommandAsync(
            SimulationCommandType,
            new
            {
                request.WorkflowKey,
                request.Payload,
                request.IdentityId,
                Mode = "simulation"
            },
            request.CorrelationId,
            whyceId: request.IdentityId.ToString(),
            cancellationToken: cancellationToken);

        // Step 3: Map engine result to view model
        return MapToResultView(engineResponse, policyPreview);
    }

    private async Task<PolicyPreview> RunPolicyPreviewAsync(
        SimulationRequest request,
        CancellationToken cancellationToken)
    {
        var intent = new ClassifiedIntent
        {
            Classification = "simulation",
            Domain = "simulation",
            WorkflowKey = request.WorkflowKey,
            Cluster = "platform",
            Subcluster = "simulation"
        };

        var route = new IntentRoute
        {
            Cluster = "platform",
            Authority = "simulation",
            SubCluster = "t3i",
            WorkflowKey = request.WorkflowKey,
            ExecutionTarget = "T3I",
            Domain = "simulation",
            CommandType = SimulationCommandType
        };

        return await _policyPreview.PreviewAsync(
            intent, route, identity: null, request.CorrelationId, cancellationToken);
    }

    private static SimulationResultView MapToResultView(ApiResponse engineResponse, PolicyPreview policyPreview)
    {
        if (engineResponse.StatusCode is < 200 or >= 300)
        {
            return new SimulationResultView
            {
                Decision = "FAIL",
                Warnings = [],
                Violations = [engineResponse.Error ?? "Simulation engine unavailable"],
                Impact = new SimulationImpactView(),
                PolicyPreview = policyPreview
            };
        }

        // Map engine result data to view
        if (engineResponse.Data is SimulationResultView existingView)
            return existingView with { PolicyPreview = policyPreview };

        return ExtractFromEngineData(engineResponse.Data, policyPreview);
    }

    private static SimulationResultView ExtractFromEngineData(object? data, PolicyPreview policyPreview)
    {
        if (data is not IDictionary<string, object?> dict)
        {
            return new SimulationResultView
            {
                Decision = "SUCCESS",
                Warnings = [],
                Violations = [],
                Impact = new SimulationImpactView(),
                PolicyPreview = policyPreview
            };
        }

        var decision = dict.TryGetValue("decision", out var d) && d is string ds ? ds : "SUCCESS";
        var warnings = dict.TryGetValue("warnings", out var w) && w is IReadOnlyList<string> ws ? ws : [];
        var violations = dict.TryGetValue("violations", out var v) && v is IReadOnlyList<string> vs ? vs : [];

        var impact = new SimulationImpactView();
        if (dict.TryGetValue("impact", out var impactObj) && impactObj is IDictionary<string, object?> impactDict)
        {
            impact = new SimulationImpactView
            {
                EstimatedCost = impactDict.TryGetValue("estimatedCost", out var ec) && ec is decimal ecd ? ecd : null,
                EstimatedRevenue = impactDict.TryGetValue("estimatedRevenue", out var er) && er is decimal erd ? erd : null,
                ExposureRisk = impactDict.TryGetValue("exposureRisk", out var ex) && ex is decimal exd ? exd : null
            };
        }

        return new SimulationResultView
        {
            Decision = decision,
            Warnings = warnings,
            Violations = violations,
            Impact = impact,
            PolicyPreview = policyPreview
        };
    }
}
