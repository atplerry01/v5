using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Observability;

/// <summary>
/// Observability middleware: captures full execution span for every command.
/// Non-blocking, non-throwing. Records trace data for audit trail.
///
/// E10 production-readiness: emits structured log entries at command entry
/// and exit with classification/context/domain routing metadata, latency,
/// and enforcement constraint state. Tracing path coverage:
/// API → Runtime (this span) → Engine → Domain → Event → Projection.
///
/// ILogger is optional — hosts without logging retain pre-E10 behavior.
/// </summary>
public sealed class TracingMiddleware : IMiddleware
{
    private static readonly ActivitySource Source = new("Whycespace.Runtime.ControlPlane");

    private readonly ILogger<TracingMiddleware>? _logger;

    public TracingMiddleware() { }

    public TracingMiddleware(ILogger<TracingMiddleware>? logger)
    {
        _logger = logger;
    }

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType().Name;

        using var activity = Source.StartActivity($"Runtime.Execute.{commandType}");
        activity?.SetTag("correlation.id", context.CorrelationId.ToString());
        activity?.SetTag("command.id", context.CommandId.ToString());
        activity?.SetTag("command.type", commandType);
        activity?.SetTag("tenant.id", context.TenantId);
        activity?.SetTag("actor.id", context.ActorId);
        activity?.SetTag("aggregate.id", context.AggregateId.ToString());
        activity?.SetTag("policy.id", context.PolicyId);
        activity?.SetTag("classification", context.Classification);
        activity?.SetTag("context", context.Context);
        activity?.SetTag("domain", context.Domain);

        _logger?.LogInformation(
            "Command dispatching: {CommandType} | CorrelationId={CorrelationId} CommandId={CommandId} " +
            "AggregateId={AggregateId} TenantId={TenantId} ActorId={ActorId} PolicyId={PolicyId} " +
            "Route={Classification}/{Context}/{Domain}",
            commandType,
            context.CorrelationId,
            context.CommandId,
            context.AggregateId,
            context.TenantId,
            context.ActorId,
            context.PolicyId,
            context.Classification,
            context.Context,
            context.Domain);

        var sw = Stopwatch.StartNew();

        try
        {
            var result = await next(cancellationToken);
            sw.Stop();

            activity?.SetTag("result.success", result.IsSuccess);
            activity?.SetTag("duration.ms", sw.Elapsed.TotalMilliseconds);

            if (result.IsSuccess)
            {
                activity?.SetTag("events.count", result.EmittedEvents?.Count ?? 0);

                _logger?.LogInformation(
                    "Command completed: {CommandType} | Success Duration={DurationMs:F1}ms " +
                    "EventsEmitted={EventCount} CorrelationId={CorrelationId} CommandId={CommandId} " +
                    "EnforcementConstraint={EnforcementConstraint}",
                    commandType,
                    sw.Elapsed.TotalMilliseconds,
                    result.EmittedEvents?.Count ?? 0,
                    context.CorrelationId,
                    context.CommandId,
                    context.EnforcementConstraint ?? "none");
            }
            else
            {
                activity?.SetTag("result.error", result.Error);

                _logger?.LogWarning(
                    "Command failed: {CommandType} | Error={Error} Duration={DurationMs:F1}ms " +
                    "CorrelationId={CorrelationId} CommandId={CommandId} " +
                    "EnforcementConstraint={EnforcementConstraint}",
                    commandType,
                    result.Error,
                    sw.Elapsed.TotalMilliseconds,
                    context.CorrelationId,
                    context.CommandId,
                    context.EnforcementConstraint ?? "none");
            }

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("duration.ms", sw.Elapsed.TotalMilliseconds);

            _logger?.LogError(ex,
                "Command exception: {CommandType} | Duration={DurationMs:F1}ms " +
                "CorrelationId={CorrelationId} CommandId={CommandId}",
                commandType,
                sw.Elapsed.TotalMilliseconds,
                context.CorrelationId,
                context.CommandId);

            throw;
        }
    }
}
