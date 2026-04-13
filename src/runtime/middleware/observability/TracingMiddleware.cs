using System.Diagnostics;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Observability;

/// <summary>
/// Observability middleware: captures full execution span for every command.
/// Non-blocking, non-throwing. Records trace data for audit trail.
/// </summary>
public sealed class TracingMiddleware : IMiddleware
{
    private static readonly ActivitySource Source = new("Whycespace.Runtime.ControlPlane");

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType().Name;

        using var activity = Source.StartActivity($"Runtime.Execute.{commandType}");
        activity?.SetTag("correlation.id", context.CorrelationId.ToString());
        activity?.SetTag("command.type", commandType);
        activity?.SetTag("tenant.id", context.TenantId);
        activity?.SetTag("actor.id", context.ActorId);
        activity?.SetTag("aggregate.id", context.AggregateId.ToString());
        activity?.SetTag("policy.id", context.PolicyId);

        try
        {
            var result = await next(cancellationToken);

            activity?.SetTag("result.success", result.IsSuccess);
            if (!result.IsSuccess)
            {
                activity?.SetTag("result.error", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
