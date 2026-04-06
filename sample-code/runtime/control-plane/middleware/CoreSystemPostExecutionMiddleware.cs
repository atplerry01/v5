using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Core-system AFTER-EXECUTION hook. Verifies state consistency and
/// emits reconciliation events after successful command execution.
///
/// Performance: anomaly emission is fire-and-forget — does not block
/// the command response. Reconciliation check runs inline but is
/// lightweight (T0U gate). Anomaly recording is async background work.
///
/// Non-blocking: response returns immediately after reconciliation check.
/// Idempotent: same execution always produces same verification result.
/// </summary>
public sealed class CoreSystemPostExecutionMiddleware : IMiddleware
{
    private readonly IValidationGate _reconciliationGate;
    private readonly ICoreSystemAnomalyEmitter _anomalyEmitter;

    public CoreSystemPostExecutionMiddleware(
        IValidationGate reconciliationGate,
        ICoreSystemAnomalyEmitter anomalyEmitter)
    {
        ArgumentNullException.ThrowIfNull(reconciliationGate);
        ArgumentNullException.ThrowIfNull(anomalyEmitter);
        _reconciliationGate = reconciliationGate;
        _anomalyEmitter = anomalyEmitter;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // Execute the command first
        var result = await next(context);

        // Only verify after successful execution
        if (!result.Success)
            return result;

        var envelope = context.Envelope;
        var entityId = envelope.Metadata.Headers.TryGetValue("x-entity-id", out var id) ? id : string.Empty;

        // Post-execution reconciliation check — lightweight T0U gate
        var reconciliationResult = await _reconciliationGate.ValidateAsync(
            envelope.CommandType, entityId, context.CancellationToken);

        if (!reconciliationResult.IsValid)
        {
            // Fire-and-forget anomaly emission — do NOT block the response.
            // Anomaly recording is durable (outbox pattern in infra) so loss is acceptable
            // only in catastrophic failure scenarios.
            _ = _anomalyEmitter.EmitAnomalyAsync(
                envelope.CommandId,
                envelope.CommandType,
                entityId,
                reconciliationResult.Reason ?? "Post-execution consistency check failed.",
                CancellationToken.None);
        }

        return result;
    }
}

/// <summary>
/// Interface for emitting core-system anomaly events when post-execution
/// verification detects inconsistencies.
/// </summary>
public interface ICoreSystemAnomalyEmitter
{
    Task EmitAnomalyAsync(
        Guid commandId,
        string commandType,
        string entityId,
        string reason,
        CancellationToken cancellationToken = default);
}
