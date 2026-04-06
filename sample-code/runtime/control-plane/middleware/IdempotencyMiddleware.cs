using Whycespace.Runtime.Command;
using Whycespace.Runtime.Idempotency;
using Whycespace.Runtime.Observability;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class IdempotencyMiddleware : IMiddleware
{
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";
    private const string KeyPrefix = "policy";

    private readonly IIdempotencyRegistry _registry;
    private readonly MetricsCollector? _metrics;

    public IdempotencyMiddleware(IIdempotencyRegistry registry, MetricsCollector? metrics = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
        _metrics = metrics;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var headers = context.Envelope.Metadata.Headers;

        // X-Idempotency-Key header takes precedence over CommandId
        if (headers.TryGetValue(IdempotencyKeyHeader, out var clientKey)
            && !string.IsNullOrWhiteSpace(clientKey))
        {
            var storageKey = $"{KeyPrefix}:{clientKey}";
            return await ExecuteWithStringKeyAsync(storageKey, context, next);
        }

        // Fallback: use CommandId for idempotency
        return await ExecuteWithCommandIdAsync(context, next);
    }

    private async Task<CommandResult> ExecuteWithStringKeyAsync(
        string storageKey, CommandContext context, MiddlewareDelegate next)
    {
        var existing = await _registry.GetResultByKeyAsync(storageKey, context.CancellationToken);
        if (existing is not null)
        {
            _metrics?.Increment(MetricsCollector.Names.IdempotencyHit,
                new Dictionary<string, string> { ["command_type"] = context.Envelope.CommandType });
            return existing;
        }

        _metrics?.Increment(MetricsCollector.Names.IdempotencyMiss,
            new Dictionary<string, string> { ["command_type"] = context.Envelope.CommandType });

        var result = await next(context);
        await _registry.RegisterByKeyAsync(storageKey, result, context.CancellationToken);
        return result;
    }

    private async Task<CommandResult> ExecuteWithCommandIdAsync(
        CommandContext context, MiddlewareDelegate next)
    {
        var commandId = context.Envelope.CommandId;

        var existing = await _registry.GetResultAsync(commandId, context.CancellationToken);
        if (existing is not null)
        {
            _metrics?.Increment(MetricsCollector.Names.IdempotencyHit,
                new Dictionary<string, string> { ["command_type"] = context.Envelope.CommandType });
            return existing;
        }

        _metrics?.Increment(MetricsCollector.Names.IdempotencyMiss,
            new Dictionary<string, string> { ["command_type"] = context.Envelope.CommandType });

        var result = await next(context);
        await _registry.RegisterAsync(commandId, result, context.CancellationToken);
        return result;
    }
}
