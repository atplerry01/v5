using System.Diagnostics;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.Observability;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class TracingMiddleware : IMiddleware
{
    private readonly TraceManager _traceManager;

    public TracingMiddleware(TraceManager traceManager)
    {
        ArgumentNullException.ThrowIfNull(traceManager);
        _traceManager = traceManager;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var envelope = context.Envelope;
        var trace = _traceManager.BeginTrace(envelope, context.ExecutionId);
        context.Set(ContextKeys.ExecutionTrace, trace);

        var commandSpan = trace.BeginSpan($"Command:{envelope.CommandType}", TraceSpanKind.Command);

        try
        {
            var result = await next(context);

            commandSpan.Complete(result.Success, result.ErrorMessage);
            _traceManager.CompleteTrace(envelope.CommandId, result.Success);

            return result;
        }
        catch (Exception ex)
        {
            commandSpan.Complete(false, ex.Message);
            _traceManager.CompleteTrace(envelope.CommandId, false);
            throw;
        }
    }

    public static class ContextKeys
    {
        public const string ExecutionTrace = "Observability.ExecutionTrace";
    }
}
