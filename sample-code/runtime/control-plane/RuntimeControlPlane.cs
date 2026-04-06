using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.Sharding;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.ControlPlane;

public sealed class RuntimeControlPlane
{
    private readonly MiddlewareDelegate _executor;
    private readonly IClock _clock;
    private readonly IPartitionKeyResolver _partitionKeyResolver;

    internal RuntimeControlPlane(
        MiddlewarePipeline pipeline,
        CommandDispatcher dispatcher,
        IClock clock,
        IPartitionKeyResolver partitionKeyResolver)
    {
        _executor = pipeline.Build(context => dispatcher.DispatchAsync(context));
        _clock = clock;
        _partitionKeyResolver = partitionKeyResolver;
    }

    public async Task<CommandResult> ExecuteAsync(CommandEnvelope envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var traceId = ResolveTraceId(envelope);
        var partitionKey = _partitionKeyResolver.Resolve(envelope);

        var context = new CommandContext
        {
            Envelope = envelope,
            ExecutionId = CommandContext.GenerateExecutionId(envelope),
            Clock = _clock,
            TraceId = traceId,
            PartitionKey = partitionKey,
            CancellationToken = cancellationToken
        };

        try
        {
            return await _executor(context);
        }
        catch (RuntimeControlPlaneException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RuntimeControlPlaneException(
                $"Unhandled error executing command {envelope.CommandId}: {ex.Message}", ex);
        }
    }

    private static string ResolveTraceId(CommandEnvelope envelope)
    {
        if (envelope.Metadata.Headers.TryGetValue("x-trace-id", out var traceId)
            && !string.IsNullOrWhiteSpace(traceId))
        {
            return traceId;
        }

        return $"trace-{envelope.CommandId:N}";
    }
}
