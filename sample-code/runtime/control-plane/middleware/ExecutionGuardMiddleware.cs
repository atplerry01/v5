using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class ExecutionGuardMiddleware : IMiddleware
{
    private readonly IExecutionGuard _guard;

    public ExecutionGuardMiddleware(IExecutionGuard guard)
    {
        ArgumentNullException.ThrowIfNull(guard);
        _guard = guard;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var envelope = context.Envelope;

        var allowed = await _guard.CanExecuteAsync(
            envelope.CommandType,
            envelope.CorrelationId,
            context.CancellationToken);

        if (!allowed)
        {
            return CommandResult.Fail(
                envelope.CommandId,
                $"Execution blocked for command type '{envelope.CommandType}'.",
                "EXECUTION_BLOCKED");
        }

        // Mark context as originating from runtime — used by EngineInvoker guard.
        context.Set(CommandContext.RuntimeOriginKey, true);

        return await next(context);
    }
}

public interface IExecutionGuard
{
    Task<bool> CanExecuteAsync(
        string commandType,
        string correlationId,
        CancellationToken cancellationToken);
}
