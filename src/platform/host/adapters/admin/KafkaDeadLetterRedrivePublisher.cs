using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Runtime.Admin;

namespace Whycespace.Platform.Host.Adapters.Admin;

/// <summary>
/// R4.B — Kafka adapter for <see cref="IDeadLetterRedrivePublisher"/>. The
/// dead-letter payload is UTF-8 JSON bytes (the canonical event envelope
/// shape), so we decode it once and hand it to the shared
/// <see cref="IProducer{TKey,TValue}"/> that the outbox relay already uses.
/// Adding a second byte-native producer would double the producer footprint
/// for a single cold-path operator action — the UTF-8 decode is explicit so
/// any future non-JSON payload class will fail fast on decode rather than
/// silently re-drive garbage.
///
/// <para>Headers carry the canonical envelope metadata (event id, type,
/// correlation id, schema version) so downstream consumers see the re-driven
/// message identically to the original.</para>
/// </summary>
internal sealed class KafkaDeadLetterRedrivePublisher : IDeadLetterRedrivePublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaDeadLetterRedrivePublisher>? _logger;

    public KafkaDeadLetterRedrivePublisher(
        IProducer<string, string> producer,
        ILogger<KafkaDeadLetterRedrivePublisher>? logger = null)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task PublishAsync(
        string topic,
        byte[] payload,
        Guid eventId,
        string eventType,
        int? schemaVersion,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new DeadLetterRedrivePublishException("Source topic is empty.");
        if (payload.Length == 0)
            throw new DeadLetterRedrivePublishException("Payload is empty.");

        string json;
        try
        {
            json = Encoding.UTF8.GetString(payload);
        }
        catch (DecoderFallbackException ex)
        {
            throw new DeadLetterRedrivePublishException(
                "Dead-letter payload is not valid UTF-8; re-drive requires a canonical JSON envelope.", ex);
        }

        var headers = new Headers
        {
            { "event-id", Encoding.UTF8.GetBytes(eventId.ToString()) },
            { "event-type", Encoding.UTF8.GetBytes(eventType) },
            { "correlation-id", Encoding.UTF8.GetBytes(correlationId.ToString()) },
            { "redrive", Encoding.UTF8.GetBytes("true") },
        };
        if (schemaVersion is not null)
            headers.Add("schema-version", Encoding.UTF8.GetBytes(schemaVersion.Value.ToString()));

        var message = new Message<string, string>
        {
            Key = eventId.ToString(),
            Value = json,
            Headers = headers,
        };

        try
        {
            await _producer.ProduceAsync(topic, message, cancellationToken);
        }
        catch (ProduceException<string, string> ex)
        {
            throw new DeadLetterRedrivePublishException(
                $"Kafka produce to '{topic}' failed: {ex.Error.Code} {ex.Error.Reason}", ex);
        }
    }
}
