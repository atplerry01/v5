using System.Diagnostics;
using System.Diagnostics.Metrics;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Observability;

/// <summary>
/// Observability middleware: records command execution metrics (duration, count, errors)
/// via System.Diagnostics.Metrics (OpenTelemetry-compatible). Runs inside the tracing
/// span so duration excludes trace bookkeeping. Non-blocking, non-throwing.
///
/// R1 Batch 4 — R-STATE-BOUNDARY-01: prior revisions carried static mutable
/// counters (<c>_totalCommands</c> et al.) and a <c>ConcurrentDictionary</c>
/// per-command snapshot exposed via <c>GetCounts</c> / <c>GetPerCommandMetrics</c>.
/// Both getters had zero callers and the counters duplicated the OpenTelemetry
/// counters already published below — removed to honor the state-boundary rule
/// and the clean-code guard.
/// </summary>
public sealed class MetricsMiddleware : IMiddleware
{
    private static readonly Meter RuntimeMeter = new("Whycespace.Runtime.ControlPlane", "1.0.0");

    private static readonly Counter<long> CommandTotal =
        RuntimeMeter.CreateCounter<long>("whyce.runtime.command.total", "commands", "Total commands dispatched");
    private static readonly Counter<long> CommandSuccess =
        RuntimeMeter.CreateCounter<long>("whyce.runtime.command.success", "commands", "Successful commands");
    private static readonly Counter<long> CommandFailed =
        RuntimeMeter.CreateCounter<long>("whyce.runtime.command.failed", "commands", "Failed commands");
    private static readonly Histogram<double> CommandDuration =
        RuntimeMeter.CreateHistogram<double>("whyce.runtime.command.duration_ms", "ms", "Command execution latency");

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType().Name;

        var tags = new TagList
        {
            { "command.type", commandType },
            { "classification", context.Classification ?? "unknown" },
            { "context", context.Context ?? "unknown" },
            { "domain", context.Domain ?? "unknown" }
        };

        CommandTotal.Add(1, tags);

        var sw = Stopwatch.StartNew();

        try
        {
            var result = await next(cancellationToken);
            sw.Stop();

            CommandDuration.Record(sw.Elapsed.TotalMilliseconds, tags);

            if (result.IsSuccess)
                CommandSuccess.Add(1, tags);
            else
                CommandFailed.Add(1, tags);

            return result;
        }
        catch
        {
            sw.Stop();
            CommandDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
            CommandFailed.Add(1, tags);
            throw;
        }
    }
}
