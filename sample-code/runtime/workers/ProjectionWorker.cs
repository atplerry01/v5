using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.Projection;
using Whycespace.Shared.Contracts.Infrastructure;
using Whycespace.Shared.Contracts.Messaging;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Workers;

/// <summary>
/// Consumes events via IMessageConsumer and builds projections.
/// No direct Kafka, SQL, or Redis — all through contracts.
/// </summary>
public class ProjectionWorker : BackgroundService
{
    private readonly ILogger<ProjectionWorker> _logger;
    private readonly IMessageConsumerFactory _consumerFactory;
    private readonly ProjectionRouter _router;
    private readonly IProjectionStore _projectionStore;
    private readonly IClock _clock;

    public ProjectionWorker(
        ILogger<ProjectionWorker> logger,
        IMessageConsumerFactory consumerFactory,
        ProjectionRouter router,
        IProjectionStore projectionStore,
        IClock clock)
    {
        _logger = logger;
        _consumerFactory = consumerFactory;
        _router = router;
        _projectionStore = projectionStore;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var domainGroups = _router.SubscribedEventTypes
            .GroupBy(ExtractDomainFromEventType)
            .ToDictionary(g => g.Key, g => g.Select(et => $"whyce.{et}").Distinct().ToList());

        var tasks = new List<Task>();
        foreach (var (domain, topics) in domainGroups)
        {
            tasks.Add(RunDomainProjectionConsumerAsync(domain, topics, stoppingToken));
        }

        _logger.LogInformation(
            "ProjectionWorker started — {Count} domain projection groups: {Domains}",
            domainGroups.Count, string.Join(", ", domainGroups.Keys));

        await Task.WhenAll(tasks);

        _logger.LogInformation("ProjectionWorker stopped");
    }

    private static string ExtractDomainFromEventType(string eventType)
    {
        var firstDot = eventType.IndexOf('.');
        return firstDot > 0 ? eventType[..firstDot] : eventType;
    }

    private async Task RunDomainProjectionConsumerAsync(
        string domain, List<string> topics, CancellationToken stoppingToken)
    {
        var groupId = $"whyce-projection-{domain}";
        await using var consumer = _consumerFactory.Create(groupId);
        consumer.Subscribe(topics);

        _logger.LogInformation(
            "Projection group '{GroupId}' subscribed to {Topics}",
            groupId, string.Join(", ", topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await consumer.ConsumeAsync(TimeSpan.FromSeconds(1), stoppingToken);
                if (message is null) continue;

                await ProcessMessageAsync(message, stoppingToken);
                consumer.Commit();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Domain}] ProjectionWorker processing error", domain);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        _logger.LogInformation("Projection group '{GroupId}' stopped", groupId);
    }

    private async Task ProcessMessageAsync(ConsumedMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonDocument.Parse(message.Value);
        var root = payload.RootElement;

        if (!root.TryGetProperty("event_id", out var eventIdProp))
        {
            _logger.LogWarning("ProjectionWorker received message without event_id on {Topic}", message.Topic);
            return;
        }

        var eventId = eventIdProp.GetGuid();
        var eventType = root.GetProperty("event_type").GetString()!;
        var eventData = root.GetProperty("event_data");
        var metadata = root.TryGetProperty("metadata", out var m) ? m : default;

        // 1. Route to handler (idempotency enforced by ProjectionRouter via IEventIdempotencyStore)
        await _router.RouteAsync(eventId, eventType, eventData, metadata, cancellationToken);

        // 3. Write to projection store
        await WriteProjectionAsync(eventType, eventData, cancellationToken);

        // 4. Save checkpoint
        await _projectionStore.SetCheckpointAsync(
            $"projection:{eventType}", message.Offset, cancellationToken);

        _logger.LogInformation(
            "ProjectionWorker processed {EventType} event {EventId}",
            eventType, eventId);
    }

    private async Task WriteProjectionAsync(
        string eventType, JsonElement eventData, CancellationToken cancellationToken)
    {
        try
        {
            var projectionName = eventType.Replace('.', ':');
            var key = ExtractProjectionKey(eventData);
            if (key is null) return;

            var data = new Dictionary<string, object?>();
            foreach (var prop in eventData.EnumerateObject())
            {
                data[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => prop.Value.ToString()
                };
            }
            data["event_type"] = eventType;
            data["projected_at"] = _clock.UtcNowOffset.ToString("o");

            await _projectionStore.SetAsync(projectionName, key, data, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write projection for {EventType}", eventType);
        }
    }

    private static string? ExtractProjectionKey(JsonElement eventData)
    {
        if (eventData.TryGetProperty("todo_id", out var todoId))
            return todoId.GetGuid().ToString();
        if (eventData.TryGetProperty("TodoId", out var todoIdUpper))
            return todoIdUpper.GetGuid().ToString();
        if (eventData.TryGetProperty("incident_id", out var incidentId))
            return incidentId.GetString();
        if (eventData.TryGetProperty("IncidentId", out var incidentIdUpper))
            return incidentIdUpper.GetString();
        if (eventData.TryGetProperty("AggregateId", out var aggId))
            return aggId.GetString();
        if (eventData.TryGetProperty("aggregate_id", out var aggIdLower))
            return aggIdLower.GetString();
        return null;
    }
}
