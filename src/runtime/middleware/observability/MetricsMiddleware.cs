using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Observability;

/// <summary>
/// Observability middleware: records command execution metrics (duration, count, errors).
/// Runs inside tracing span so duration excludes trace bookkeeping.
/// Non-blocking, non-throwing.
///
/// E10 production-readiness: exposes per-command-type counters and latency
/// via System.Diagnostics.Metrics (OpenTelemetry-compatible) alongside the
/// existing global counters. Instruments are created once per command type
/// and cached for the process lifetime.
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

    private static long _totalCommands;
    private static long _successfulCommands;
    private static long _failedCommands;

    private static readonly ConcurrentDictionary<string, CommandTypeMetrics> PerCommandMetrics = new();

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType().Name;
        var metrics = PerCommandMetrics.GetOrAdd(commandType, _ => new CommandTypeMetrics());

        Interlocked.Increment(ref _totalCommands);
        metrics.IncrementTotal();

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

            var durationMs = sw.Elapsed.TotalMilliseconds;
            metrics.RecordLatency(durationMs);
            CommandDuration.Record(durationMs, tags);

            if (result.IsSuccess)
            {
                Interlocked.Increment(ref _successfulCommands);
                metrics.IncrementSuccess();
                CommandSuccess.Add(1, tags);
            }
            else
            {
                Interlocked.Increment(ref _failedCommands);
                metrics.IncrementFailed();
                CommandFailed.Add(1, tags);
            }

            return result;
        }
        catch
        {
            sw.Stop();
            var durationMs = sw.Elapsed.TotalMilliseconds;
            metrics.RecordLatency(durationMs);
            CommandDuration.Record(durationMs, tags);

            Interlocked.Increment(ref _failedCommands);
            metrics.IncrementFailed();
            CommandFailed.Add(1, tags);

            throw;
        }
    }

    public static (long Total, long Success, long Failed) GetCounts() =>
        (Interlocked.Read(ref _totalCommands),
         Interlocked.Read(ref _successfulCommands),
         Interlocked.Read(ref _failedCommands));

    public static IReadOnlyDictionary<string, CommandTypeSnapshot> GetPerCommandMetrics()
    {
        var snapshot = new Dictionary<string, CommandTypeSnapshot>(PerCommandMetrics.Count);
        foreach (var (key, value) in PerCommandMetrics)
            snapshot[key] = value.Snapshot();
        return snapshot;
    }

    public sealed record CommandTypeSnapshot(long Total, long Success, long Failed, double LastLatencyMs);

    private sealed class CommandTypeMetrics
    {
        private long _total;
        private long _success;
        private long _failed;
        private double _lastLatencyMs;

        public void IncrementTotal() => Interlocked.Increment(ref _total);
        public void IncrementSuccess() => Interlocked.Increment(ref _success);
        public void IncrementFailed() => Interlocked.Increment(ref _failed);
        public void RecordLatency(double ms) => Volatile.Write(ref _lastLatencyMs, ms);

        public CommandTypeSnapshot Snapshot() => new(
            Interlocked.Read(ref _total),
            Interlocked.Read(ref _success),
            Interlocked.Read(ref _failed),
            Volatile.Read(ref _lastLatencyMs));
    }
}
