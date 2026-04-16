using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Reconciliation lifecycle handler — T1M-tier orchestrator that reacts to
/// reconciliation domain events and dispatches the next T2E command through
/// <see cref="ISystemIntentDispatcher"/>. Implements the state machine
/// described in Phase 2.5:
///
///   ReconciliationTriggeredEvent   -> MarkMismatchedCommand       (attempt match)
///   ReconciliationMismatchedEvent  -> DetectDiscrepancyCommand    (detect discrepancy)
///   DiscrepancyDetectedEvent       -> InvestigateDiscrepancyCommand
///   DiscrepancyInvestigatedEvent   -> ResolveDiscrepancyCommand
///   DiscrepancyResolvedEvent       -> ResolveReconciliationCommand (close the loop)
///
/// Phase 2.6 hardening:
///   - Every <c>DispatchAsync</c> call is wrapped in <see cref="DispatchOrThrow"/>
///     which inspects <c>result.IsSuccess</c>, logs any failure, and throws on
///     non-success so the worker can route to its visible-error path. Silent
///     dispatch failures are no longer possible.
///   - Transient timeouts are retried once after a 100 ms backoff. Policy
///     denials are NOT retried (retry would not change the outcome and would
///     mask a real guard-rail signal).
///
/// Non-negotiable rules preserved:
///   - No domain logic in the handler (only event -> command routing).
///   - No aggregate mutation (all mutation happens via T2E handlers).
///   - All transitions dispatched via ISystemIntentDispatcher.
///   - Deterministic: id derivations are SHA256(seed) via IIdGenerator.
///   - Replay-safe: identical input event produces identical output command.
/// </summary>
public sealed class ReconciliationLifecycleHandler
{
    private static readonly DomainRoute ProcessRoute     = new("economic", "reconciliation", "process");
    private static readonly DomainRoute DiscrepancyRoute = new("economic", "reconciliation", "discrepancy");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IReconciliationWorkflowLookup _lookup;
    private readonly ILogger<ReconciliationLifecycleHandler>? _logger;

    public ReconciliationLifecycleHandler(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IReconciliationWorkflowLookup lookup,
        ILogger<ReconciliationLifecycleHandler>? logger = null)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _lookup = lookup;
        _logger = logger;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        switch (envelope.EventType)
        {
            case "ReconciliationTriggeredEvent":
            {
                var cmd = new MarkMismatchedCommand(envelope.AggregateId);
                await DispatchOrThrow(cmd, ProcessRoute, envelope.CorrelationId, cancellationToken);
                break;
            }

            case "ReconciliationMismatchedEvent":
            {
                var discrepancyId = _idGenerator.Generate(
                    $"economic:reconciliation:discrepancy:{envelope.AggregateId}:auto");

                var cmd = new DetectDiscrepancyCommand(
                    discrepancyId,
                    envelope.AggregateId,
                    "Projection",
                    ExpectedValue: 0m,
                    ActualValue: 0m,
                    Difference: 0m,
                    DetectedAt: _clock.UtcNow);

                await DispatchOrThrow(cmd, DiscrepancyRoute, envelope.CorrelationId, cancellationToken);
                break;
            }

            case "DiscrepancyDetectedEvent":
            {
                var cmd = new InvestigateDiscrepancyCommand(envelope.AggregateId);
                await DispatchOrThrow(cmd, DiscrepancyRoute, envelope.CorrelationId, cancellationToken);
                break;
            }

            case "DiscrepancyInvestigatedEvent":
            {
                var cmd = new ResolveDiscrepancyCommand(
                    envelope.AggregateId,
                    "Auto-resolved by reconciliation lifecycle workflow.");
                await DispatchOrThrow(cmd, DiscrepancyRoute, envelope.CorrelationId, cancellationToken);
                break;
            }

            case "DiscrepancyResolvedEvent":
            {
                var processId = await _lookup.FindProcessIdByDiscrepancyAsync(
                    envelope.AggregateId, cancellationToken);

                if (processId is null || processId == Guid.Empty)
                {
                    _logger?.LogWarning(
                        "No workflow row found for DiscrepancyId={DiscrepancyId}; cannot close the reconciliation loop. Ignoring.",
                        envelope.AggregateId);
                    return;
                }

                var cmd = new ResolveReconciliationCommand(processId.Value);
                await DispatchOrThrow(cmd, ProcessRoute, envelope.CorrelationId, cancellationToken);
                break;
            }
        }
    }

    /// <summary>
    /// Dispatches the command, checks <c>result.IsSuccess</c>, logs any
    /// failure at Error level, and throws on non-success. A single retry is
    /// attempted only for transient timeout errors — policy denies are not
    /// retried so guard-rail signals remain visible.
    /// </summary>
    private async Task DispatchOrThrow(
        object command,
        DomainRoute route,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var result = await _dispatcher.DispatchAsync(command, route, cancellationToken);

        if (!result.IsSuccess && LooksLikeTransient(result.Error))
        {
            _logger?.LogWarning(
                "Workflow dispatch transient failure; retrying after 100ms. Command: {Command}, CorrelationId: {CorrelationId}, Error: {Error}",
                command.GetType().Name, correlationId, result.Error);

            await Task.Delay(100, cancellationToken);
            result = await _dispatcher.DispatchAsync(command, route, cancellationToken);
        }

        if (!result.IsSuccess)
        {
            _logger?.LogError(
                "Workflow dispatch failed. Command: {Command}, CorrelationId: {CorrelationId}, Error: {Error}",
                command.GetType().Name, correlationId, result.Error);

            throw new InvalidOperationException(
                $"Workflow dispatch failed for {command.GetType().Name}: {result.Error}");
        }
    }

    private static bool LooksLikeTransient(string? error)
    {
        if (string.IsNullOrEmpty(error)) return false;
        return error.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || error.Contains("timed out", StringComparison.OrdinalIgnoreCase)
            || error.Contains("breaker_open", StringComparison.OrdinalIgnoreCase)
            || error.Contains("unavailable", StringComparison.OrdinalIgnoreCase);
    }
}
