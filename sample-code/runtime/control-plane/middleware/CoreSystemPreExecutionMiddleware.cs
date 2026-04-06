using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Core-system BEFORE-EXECUTION hook. Validates invariants and rejects invalid commands.
/// Runs financial control, temporal, and system state validation gates
/// BEFORE the command reaches the engine.
///
/// Performance: gates run concurrently via Task.WhenAll to minimize latency.
/// Non-blocking: validation gates are lightweight T0U checks.
/// Deterministic: same input always produces same validation result.
/// </summary>
public sealed class CoreSystemPreExecutionMiddleware : IMiddleware
{
    private readonly IValidationGate _financialGate;
    private readonly IValidationGate _temporalGate;
    private readonly IValidationGate _systemStateGate;

    public CoreSystemPreExecutionMiddleware(
        IValidationGate financialGate,
        IValidationGate temporalGate,
        IValidationGate systemStateGate)
    {
        ArgumentNullException.ThrowIfNull(financialGate);
        ArgumentNullException.ThrowIfNull(temporalGate);
        ArgumentNullException.ThrowIfNull(systemStateGate);
        _financialGate = financialGate;
        _temporalGate = temporalGate;
        _systemStateGate = systemStateGate;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var envelope = context.Envelope;
        var entityId = envelope.Metadata.Headers.TryGetValue("x-entity-id", out var id) ? id : string.Empty;
        var ct = context.CancellationToken;

        // Run all three gates concurrently — no dependencies between them
        var financialTask = _financialGate.ValidateAsync(envelope.CommandType, entityId, ct);
        var temporalTask = _temporalGate.ValidateAsync(envelope.CommandType, entityId, ct);
        var stateTask = _systemStateGate.ValidateAsync(envelope.CommandType, entityId, ct);

        await Task.WhenAll(financialTask, temporalTask, stateTask);

        var financialResult = financialTask.Result;
        if (!financialResult.IsValid)
        {
            return CommandResult.Fail(
                envelope.CommandId,
                financialResult.Reason ?? "Financial control invariant violation.",
                "CORESYSTEM_FINANCIAL_REJECTED");
        }

        var temporalResult = temporalTask.Result;
        if (!temporalResult.IsValid)
        {
            return CommandResult.Fail(
                envelope.CommandId,
                temporalResult.Reason ?? "Temporal ordering invariant violation.",
                "CORESYSTEM_TEMPORAL_REJECTED");
        }

        var stateResult = stateTask.Result;
        if (!stateResult.IsValid)
        {
            return CommandResult.Fail(
                envelope.CommandId,
                stateResult.Reason ?? "System state invariant violation.",
                "CORESYSTEM_STATE_REJECTED");
        }

        // All pre-execution gates passed — proceed to engine
        return await next(context);
    }
}
