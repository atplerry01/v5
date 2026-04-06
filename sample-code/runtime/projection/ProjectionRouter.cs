using System.Text.Json;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric.Consumers;
using Whycespace.Runtime.Routing;

namespace Whycespace.Runtime.Projection;

public sealed class ProjectionRouter
{
    private readonly Dictionary<string, List<IProjectionHandler>> _handlers = new();
    private readonly DomainRouteResolver _routeResolver;
    private readonly IEventIdempotencyStore _idempotencyStore;
    private readonly ILogger<ProjectionRouter> _logger;

    public ProjectionRouter(
        IEnumerable<IProjectionHandler> handlers,
        DomainRouteResolver routeResolver,
        IEventIdempotencyStore idempotencyStore,
        ILogger<ProjectionRouter> logger)
    {
        _routeResolver = routeResolver;
        _idempotencyStore = idempotencyStore;
        _logger = logger;

        foreach (var handler in handlers)
        {
            if (!_handlers.TryGetValue(handler.EventType, out var list))
            {
                list = new List<IProjectionHandler>();
                _handlers[handler.EventType] = list;
            }
            list.Add(handler);
        }

        _logger.LogInformation(
            "ProjectionRouter initialized with {HandlerCount} handlers for {EventTypeCount} event types across {RouteCount} domain routes",
            _handlers.Values.Sum(l => l.Count), _handlers.Count, _routeResolver.RouteCount);
    }

    public async Task RouteAsync(
        Guid eventId,
        string eventType,
        JsonElement eventData,
        JsonElement metadata,
        CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("No projection handler for event type {EventType}", eventType);
            return;
        }

        // P7: EventId idempotency — skip already-processed events
        if (await _idempotencyStore.ExistsAsync(eventId, cancellationToken))
        {
            _logger.LogDebug("Skipping duplicate event {EventId} for {EventType}", eventId, eventType);
            return;
        }

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(eventData, metadata, cancellationToken);
        }

        // Mark as processed AFTER all handlers succeed
        await _idempotencyStore.MarkProcessedAsync(eventId, cancellationToken);
    }

    public DomainRoute? ResolveDomainRoute(string eventType)
    {
        return _routeResolver.Resolve(eventType);
    }

    public IReadOnlyCollection<string> SubscribedEventTypes => _handlers.Keys;
}
