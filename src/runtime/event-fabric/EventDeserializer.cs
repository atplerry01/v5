using System.Text.Json;
using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Accepts a Guid encoded either as a raw JSON string ("...") or as a value-object
/// envelope ({"value":"..."}). Required because domain events expose AggregateId as
/// a value object whose default System.Text.Json shape is the envelope form, while
/// inbound schema contracts declare AggregateId as a raw Guid.
/// </summary>
internal sealed class FlexibleGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetGuid();

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return prop.Value.GetGuid();
            }
        }

        throw new JsonException("Expected Guid string or { value: Guid } object.");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}

/// <summary>
/// Accepts an int encoded either as a raw JSON number (42) or as a value-object
/// envelope ({"value":42}). Required because domain events may expose position/count
/// fields as value objects (e.g. KanbanPosition) whose default System.Text.Json shape
/// is the envelope form, while inbound schema contracts declare them as raw ints.
/// </summary>
internal sealed class FlexibleIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32();

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return prop.Value.GetInt32();
            }
        }

        throw new JsonException("Expected int number or { value: int } object.");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}

/// <summary>
/// Reads an AggregateId from either a raw Guid string or a {"Value":"..."} envelope.
/// Required because the event store stores the payload-mapped form (raw Guid) while
/// DeserializeStored targets the domain event type which declares AggregateId as a
/// readonly record struct.
/// </summary>
internal sealed class AggregateIdJsonConverter : JsonConverter<AggregateId>
{
    public override AggregateId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new AggregateId(reader.GetGuid());

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new AggregateId(prop.Value.GetGuid());
            }
        }

        throw new JsonException("Expected Guid string or { Value: Guid } object for AggregateId.");
    }

    public override void Write(Utf8JsonWriter writer, AggregateId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class KanbanListIdJsonConverter : JsonConverter<KanbanListId>
{
    public override KanbanListId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new KanbanListId(reader.GetGuid());

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new KanbanListId(prop.Value.GetGuid());
            }
        }

        throw new JsonException("Expected Guid string or { Value: Guid } object for KanbanListId.");
    }

    public override void Write(Utf8JsonWriter writer, KanbanListId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class KanbanCardIdJsonConverter : JsonConverter<KanbanCardId>
{
    public override KanbanCardId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new KanbanCardId(reader.GetGuid());

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new KanbanCardId(prop.Value.GetGuid());
            }
        }

        throw new JsonException("Expected Guid string or { Value: Guid } object for KanbanCardId.");
    }

    public override void Write(Utf8JsonWriter writer, KanbanCardId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class KanbanPositionJsonConverter : JsonConverter<KanbanPosition>
{
    public override KanbanPosition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new KanbanPosition(reader.GetInt32());

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new KanbanPosition(prop.Value.GetInt32());
            }
        }

        throw new JsonException("Expected int number or { Value: int } object for KanbanPosition.");
    }

    public override void Write(Utf8JsonWriter writer, KanbanPosition value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

/// <summary>
/// Schema-driven event deserializer.
/// Replaces the static EventTypeResolver and the per-domain switch in KafkaProjectionConsumerWorker.
///
/// Two paths:
///   - DeserializeStored: event store replay (returns domain event type)
///   - DeserializeInbound: Kafka consumer (returns schema contract type, post payload mapping)
///
/// Both paths look up CLR types from EventSchemaRegistry — no per-domain branching.
/// </summary>
public sealed class EventDeserializer
{
    private readonly EventSchemaRegistry _schema;

    /// <summary>
    /// Options for deserializing stored events back into domain event types.
    /// The event store persists the payload-mapped form (schema contracts with raw
    /// Guid/int fields), but DeserializeStored targets the domain event types which
    /// declare value objects (AggregateId, KanbanListId, etc.). These converters
    /// bridge the raw primitives back into their value object wrappers.
    /// </summary>
    private static readonly JsonSerializerOptions StoredOptions = new()
    {
        Converters =
        {
            new AggregateIdJsonConverter(),
            new KanbanListIdJsonConverter(),
            new KanbanCardIdJsonConverter(),
            new KanbanPositionJsonConverter()
        }
    };

    private static readonly JsonSerializerOptions InboundOptions = new()
    {
        Converters = { new FlexibleGuidConverter(), new FlexibleIntConverter() }
    };

    public EventDeserializer(EventSchemaRegistry schema)
    {
        _schema = schema;
    }

    public object DeserializeStored(string eventType, string payload)
    {
        var entry = _schema.Resolve(eventType);
        if (entry.StoredEventType is null)
            throw new InvalidOperationException(
                $"EventSchemaRegistry has no StoredEventType registered for '{eventType}'. " +
                $"Register via the (string, EventVersion, Type stored, Type inbound) overload.");

        return JsonSerializer.Deserialize(payload, entry.StoredEventType, StoredOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize stored event '{eventType}'.");
    }

    public object DeserializeInbound(string eventType, string payload)
    {
        var entry = _schema.Resolve(eventType);
        if (entry.InboundEventType is null)
            throw new InvalidOperationException(
                $"EventSchemaRegistry has no InboundEventType registered for '{eventType}'. " +
                $"Register via the (string, EventVersion, Type stored, Type inbound) overload.");

        return JsonSerializer.Deserialize(payload, entry.InboundEventType, InboundOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize inbound event '{eventType}'.");
    }
}
