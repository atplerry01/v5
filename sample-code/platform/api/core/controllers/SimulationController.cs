using System.Text.Json;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Simulation;
using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Platform.Api.Core.Services.Simulation;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus simulation interface controller.
/// READ-ONLY T3I simulation — predicts outcomes BEFORE execution.
///
/// PLATFORM GUARDS:
/// - POST only (simulation request carries a payload body)
/// - Identity required (WhyceId must be present)
/// - RBAC + TrustScore + Consent ("simulation.access") enforced
/// - No state mutation, no events, no workflow triggers
/// - Calls T3I engines ONLY via adapter — NEVER T2E
/// - Deterministic: same input produces same output
///
/// HARD SAFETY RULES:
/// - MUST NOT mutate any state
/// - MUST NOT emit events
/// - MUST NOT trigger workflows
/// - MUST NOT write to DB
/// - MUST NOT call T2E engines
///
/// Endpoint:
///   POST /api/simulation
/// </summary>
public sealed class SimulationController
{
    private const decimal MinimumTrustScore = 0.5m;
    private static readonly IReadOnlyList<string> RequiredConsents = ["simulation.access"];

    private readonly ISimulationService _simulationService;

    public SimulationController(ISimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    /// <summary>
    /// Handles simulation requests.
    /// Enforces POST-only, identity, RBAC, TrustScore, and consent guards.
    /// </summary>
    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: POST-only enforcement
        if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "Simulation interface accepts POST only", request.TraceId);

        // Guard: Full platform guard enforcement (identity, trust, consent)
        var guardViolation = PlatformGuard.EnforceAll(
            request, MinimumTrustScore, RequiredConsents);
        if (guardViolation is not null)
            return guardViolation;

        // Parse simulation request from body
        var simulationRequest = ParseRequest(request);
        if (simulationRequest is null)
            return ApiResponse.BadRequest(
                "Invalid simulation request — workflowKey, payload, and identityId are required",
                request.TraceId);

        // Validate workflowKey is non-empty
        if (string.IsNullOrWhiteSpace(simulationRequest.WorkflowKey))
            return ApiResponse.BadRequest(
                "Invalid simulation request — workflowKey must not be empty",
                request.TraceId);

        // Execute simulation (read-only, T3I only)
        var result = await _simulationService.SimulateAsync(simulationRequest, cancellationToken);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;

        return ApiResponse.Ok(
            WhyceResponse.Ok(result, correlationId, request.TraceId),
            request.TraceId);
    }

    private static SimulationRequest? ParseRequest(ApiRequest request)
    {
        if (request.Body is null)
            return null;

        try
        {
            var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;

            // Handle already-typed SimulationRequest
            if (request.Body is SimulationRequest typed)
                return typed with { CorrelationId = correlationId };

            // Handle dictionary-style payload
            if (request.Body is IDictionary<string, object?> dict)
            {
                var workflowKey = dict.TryGetValue("workflowKey", out var wk) ? wk?.ToString() : null;
                var payload = dict.TryGetValue("payload", out var p) ? p : null;
                var identityId = dict.TryGetValue("identityId", out var id) && id is string idStr
                    ? Guid.TryParse(idStr, out var guid) ? guid : (Guid?)null
                    : null;

                if (workflowKey is null || payload is null || identityId is null)
                    return null;

                return new SimulationRequest
                {
                    WorkflowKey = workflowKey,
                    Payload = payload,
                    IdentityId = identityId.Value,
                    CorrelationId = correlationId
                };
            }

            // Handle JsonElement (from System.Text.Json deserialization)
            if (request.Body is JsonElement json && json.ValueKind == JsonValueKind.Object)
            {
                var workflowKey = json.TryGetProperty("workflowKey", out var wkProp) ? wkProp.GetString() : null;
                var payload = json.TryGetProperty("payload", out var pProp) ? (object)pProp : null;
                var identityId = json.TryGetProperty("identityId", out var idProp)
                    ? Guid.TryParse(idProp.GetString(), out var guid) ? guid : (Guid?)null
                    : null;

                if (workflowKey is null || payload is null || identityId is null)
                    return null;

                return new SimulationRequest
                {
                    WorkflowKey = workflowKey,
                    Payload = payload,
                    IdentityId = identityId.Value,
                    CorrelationId = correlationId
                };
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
