using System.Diagnostics;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.Observability;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class MetricsMiddleware : IMiddleware
{
    private readonly MetricsCollector _metrics;

    public MetricsMiddleware(MetricsCollector metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        _metrics = metrics;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var commandType = context.Envelope.CommandType;
        var tags = new Dictionary<string, string> { ["command_type"] = commandType };

        _metrics.Increment(MetricsCollector.Names.CommandReceived, tags);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next(context);

            stopwatch.Stop();

            if (result.Success)
            {
                _metrics.Increment(MetricsCollector.Names.CommandSucceeded, tags);
            }
            else
            {
                _metrics.Increment(MetricsCollector.Names.CommandFailed, tags);
            }

            _metrics.RecordDuration(MetricsCollector.Names.CommandDuration, stopwatch.Elapsed, tags);

            return result;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _metrics.Increment(MetricsCollector.Names.CommandFailed, tags);
            _metrics.RecordDuration(MetricsCollector.Names.CommandDuration, stopwatch.Elapsed, tags);
            throw;
        }
    }
}
