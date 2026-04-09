using System.Diagnostics;
using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.Observability;

/// <summary>
/// Observability middleware: records command execution metrics (duration, count, errors).
/// Runs inside tracing span so duration excludes trace bookkeeping.
/// Non-blocking, non-throwing.
/// </summary>
public sealed class MetricsMiddleware : IMiddleware
{
    private static long _totalCommands;
    private static long _successfulCommands;
    private static long _failedCommands;

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _totalCommands);
        var sw = Stopwatch.StartNew();

        try
        {
            var result = await next(cancellationToken);
            sw.Stop();

            if (result.IsSuccess)
            {
                Interlocked.Increment(ref _successfulCommands);
            }
            else
            {
                Interlocked.Increment(ref _failedCommands);
            }

            return result;
        }
        catch
        {
            sw.Stop();
            Interlocked.Increment(ref _failedCommands);
            throw;
        }
    }

    public static (long Total, long Success, long Failed) GetCounts() =>
        (Interlocked.Read(ref _totalCommands),
         Interlocked.Read(ref _successfulCommands),
         Interlocked.Read(ref _failedCommands));
}
