using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class ValidationMiddleware : IMiddleware
{
    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var envelope = context.Envelope;

        if (string.IsNullOrWhiteSpace(envelope.CommandType))
        {
            return CommandResult.Fail(envelope.CommandId, "CommandType is required.", "VALIDATION_FAILED");
        }

        if (envelope.Payload is null)
        {
            return CommandResult.Fail(envelope.CommandId, "Payload is required.", "VALIDATION_FAILED");
        }

        if (string.IsNullOrWhiteSpace(envelope.CorrelationId))
        {
            return CommandResult.Fail(envelope.CommandId, "CorrelationId is required.", "VALIDATION_FAILED");
        }

        if (envelope.Metadata is null)
        {
            return CommandResult.Fail(envelope.CommandId, "Metadata is required.", "VALIDATION_FAILED");
        }

        return await next(context);
    }
}
