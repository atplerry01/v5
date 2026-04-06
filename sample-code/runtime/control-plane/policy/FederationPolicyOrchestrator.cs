using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Orchestrates federation-aware policy evaluation through the control plane.
/// Bridges runtime to the federation engine without the engine knowing about runtime.
/// Uses shared contract types (DTOs) only — no domain entity imports.
/// RUNTIME ONLY — orchestration, no business logic.
/// </summary>
public sealed class FederationPolicyOrchestrator
{
    private readonly IPolicyFederationEngine _federationEngine;
    private readonly FederationContextInjector _contextInjector;

    /// <summary>
    /// Well-known header key for federation graph hash.
    /// Commands carrying this header trigger federated evaluation.
    /// </summary>
    public const string FederationGraphHashHeader = "X-Federation-GraphHash";

    /// <summary>
    /// Well-known property key for storing federation evaluation result on context.
    /// </summary>
    public const string FederationResultKey = "Policy.FederationResult";

    public FederationPolicyOrchestrator(
        IPolicyFederationEngine federationEngine,
        FederationContextInjector contextInjector)
    {
        ArgumentNullException.ThrowIfNull(federationEngine);
        ArgumentNullException.ThrowIfNull(contextInjector);
        _federationEngine = federationEngine;
        _contextInjector = contextInjector;
    }

    public async Task<FederationEvaluationResult?> EvaluateFederatedAsync(
        CommandContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 1. Check if context carries a federation graph hash via headers
        var headers = context.Envelope.Metadata.Headers;
        if (!headers.TryGetValue(FederationGraphHashHeader, out var graphHash)
            || string.IsNullOrWhiteSpace(graphHash))
        {
            return null; // No federation, skip
        }

        // 2. Resolve graph from repository
        var graph = await _contextInjector.ResolveFederationGraphAsync(graphHash, ct);
        if (graph is null)
            return null;

        // 3. Build evaluation input from command context
        var envelope = context.Envelope;

        var actorId = envelope.Metadata.WhyceId ?? string.Empty;
        var action = envelope.CommandType;
        var environment = headers.TryGetValue("X-Environment", out var env) ? env : "production";

        var input = new FederationEvaluationInput(
            graphHash,
            actorId,
            action,
            envelope.AggregateId ?? string.Empty,
            environment,
            context.Clock.UtcNow);

        // 4. Evaluate via federation engine
        var result = await _federationEngine.EvaluateAsync(input, ct);

        // 5. Attach result to context for downstream middleware/engines
        context.Set(FederationResultKey, result);

        return result;
    }
}
