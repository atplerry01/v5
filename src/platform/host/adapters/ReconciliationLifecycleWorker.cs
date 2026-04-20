using System.Security.Claims;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// BackgroundService that subscribes to the reconciliation event topics
/// (process + discrepancy) and routes each envelope through
/// <see cref="ReconciliationLifecycleHandler"/>. Mirrors
/// <see cref="LedgerToCapitalIntegrationWorker"/> in shape; runs on its
/// own consumer group so commit progress is independent of the
/// projection pipeline.
///
/// Subscribed topics:
///   - whyce.economic.reconciliation.process.events
///   - whyce.economic.reconciliation.discrepancy.events
///
/// Transaction boundary: events on these topics are produced by the
/// outbox relay after the aggregate's write transaction commits, so the
/// worker only ever acts on already-committed flows.
/// </summary>
public sealed class ReconciliationLifecycleWorker : BackgroundService
{
    private static readonly string[] Topics =
    [
        "whyce.economic.reconciliation.process.events",
        "whyce.economic.reconciliation.discrepancy.events"
    ];

    private const string ConsumerGroup = "whyce.integration.reconciliation-lifecycle";
    private const string WorkerName = "reconciliation-lifecycle"; // R2.E.1 rebalance-metric tag

    private readonly string _kafkaBootstrapServers;
    private readonly EventDeserializer _deserializer;
    private readonly ReconciliationLifecycleHandler _handler;
    private readonly IClock _clock;
    private readonly KafkaConsumerOptions _consumerOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ReconciliationLifecycleWorker>? _logger;

    // Canonical service-identity for the reconciliation lifecycle worker.
    // Declared per INV-202 ("System-level operations MUST execute under
    // declared service identities, not anonymous contexts"). The role is
    // "operator" because the reconciliation rego policies gate on that
    // role for auto-progression.
    private const string ServiceActorId = "system-reconciliation-lifecycle";
    private static readonly string[] ServiceRoles = ["operator"];

    public ReconciliationLifecycleWorker(
        string kafkaBootstrapServers,
        EventDeserializer deserializer,
        ReconciliationLifecycleHandler handler,
        IClock clock,
        KafkaConsumerOptions consumerOptions,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ReconciliationLifecycleWorker>? logger = null)
    {
        _kafkaBootstrapServers = kafkaBootstrapServers;
        _deserializer = deserializer;
        _handler = handler;
        _clock = clock;
        _consumerOptions = consumerOptions;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            QueuedMaxMessagesKbytes = _consumerOptions.QueuedMaxMessagesKbytes,
            MessageMaxBytes = _consumerOptions.FetchMessageMaxBytes,
            MaxPollIntervalMs = _consumerOptions.MaxPollIntervalMs,
            SessionTimeoutMs = _consumerOptions.SessionTimeoutMs,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky, // R2.E.1
        };

        var consumerBuilder = new ConsumerBuilder<string, string>(config);
        KafkaRebalanceObservability.Attach(consumerBuilder, Topics[0], WorkerName, _logger);
        using var consumer = consumerBuilder.Build();
        consumer.Subscribe(Topics);

        _logger?.LogInformation(
            "ReconciliationLifecycleWorker subscribed to {TopicCount} topics under consumer group {Group}.",
            Topics.Length, ConsumerGroup);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null)
                    continue;

                KafkaLagObservability.Record(consumer, result, WorkerName, Topics[0]);

                var eventType = ExtractHeader(result.Message.Headers, "event-type");
                var eventIdHeader = ExtractHeader(result.Message.Headers, "event-id");
                var aggregateIdHeader = ExtractHeader(result.Message.Headers, "aggregate-id");
                var correlationHeader = ExtractHeader(result.Message.Headers, "correlation-id");
                var causationHeader = ExtractHeader(result.Message.Headers, "causation-id");

                if (string.IsNullOrEmpty(eventType)
                    || string.IsNullOrEmpty(eventIdHeader)
                    || string.IsNullOrEmpty(aggregateIdHeader)
                    || !Guid.TryParse(eventIdHeader, out var parsedEventId)
                    || !Guid.TryParse(aggregateIdHeader, out var parsedAggregateId))
                {
                    _logger?.LogWarning(
                        "Skipping malformed message on {Topic}: missing or unparseable headers.",
                        result.Topic);
                    consumer.Commit(result);
                    continue;
                }

                var payload = _deserializer.DeserializeInbound(eventType, result.Message.Value);

                var envelope = new EventEnvelope
                {
                    EventId = parsedEventId,
                    AggregateId = parsedAggregateId,
                    CorrelationId = Guid.TryParse(correlationHeader, out var cid) ? cid : Guid.Empty,
                    CausationId = Guid.TryParse(causationHeader, out var causId) ? causId : Guid.Empty,
                    EventType = eventType,
                    EventName = eventType,
                    EventVersion = EventVersion.Default,
                    SchemaHash = string.Empty,
                    Payload = payload,
                    ExecutionHash = string.Empty,
                    PolicyHash = string.Empty,
                    Timestamp = _clock.UtcNow
                };

                try
                {
                    using (BeginServiceActorScope())
                    {
                        await _handler.HandleAsync(envelope, stoppingToken);
                    }
                }
                catch (Exception handlerEx)
                {
                    // Handler failures are usually transient or stale-data
                    // (e.g. a redelivered trigger whose aggregate no longer
                    // exists). Log, then commit the offset so the poll loop
                    // does not starve on one bad message. Downstream
                    // aggregates are idempotent per INV-302 so re-processing
                    // a successful step again is safe, but replaying a bad
                    // step forever blocks lifecycle progression.
                    _logger?.LogWarning(handlerEx,
                        "ReconciliationLifecycleHandler failed for {EventType} on aggregate {AggregateId}; advancing offset.",
                        envelope.EventType, envelope.AggregateId);
                }

                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ReconciliationLifecycleWorker iteration failed; will continue.");
                // Offset is NOT committed — message will be re-delivered after restart.
            }
        }

        consumer.Close();
    }

    /// <summary>
    /// Establishes a synthetic <see cref="HttpContext"/> on the async-local
    /// <see cref="IHttpContextAccessor"/> so <c>HttpCallerIdentityAccessor</c>
    /// can resolve the declared service actor for this worker. Per INV-202
    /// background operations MUST run under a declared service identity.
    /// The scope is disposed as soon as the handler returns, restoring any
    /// prior context (typically null for a non-HTTP worker).
    /// </summary>
    private IDisposable BeginServiceActorScope()
    {
        var previous = _httpContextAccessor.HttpContext;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, ServiceActorId),
            new("sub", ServiceActorId),
        };
        foreach (var role in ServiceRoles)
            claims.Add(new Claim("roles", role));

        var identity = new ClaimsIdentity(claims, authenticationType: "System");
        var principal = new ClaimsPrincipal(identity);

        _httpContextAccessor.HttpContext = new DefaultHttpContext { User = principal };

        return new RestoreScope(_httpContextAccessor, previous);
    }

    private sealed class RestoreScope : IDisposable
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly HttpContext? _previous;
        public RestoreScope(IHttpContextAccessor accessor, HttpContext? previous)
        {
            _accessor = accessor;
            _previous = previous;
        }
        public void Dispose() => _accessor.HttpContext = _previous;
    }

    private static string? ExtractHeader(Headers? headers, string key)
    {
        if (headers is null) return null;
        foreach (var h in headers)
        {
            if (string.Equals(h.Key, key, StringComparison.Ordinal))
            {
                var bytes = h.GetValueBytes();
                return bytes is null ? null : System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
        return null;
    }
}
