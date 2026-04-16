using System.Text.Json;
using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;
using Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy;
using Whycespace.Domain.EconomicSystem.Reconciliation.Process;
using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.EconomicSystem.Vault.Account;

namespace Whycespace.Runtime.EventFabric;

// ── Phase 2.6 hardening: JSON converters for reconciliation value objects.
//
// The event store persists the payload-mapped schema form (raw Guids /
// decimals). DeserializeStored targets the domain event type which declares
// these value objects. These converters bridge the stored primitives back
// into their wrapper struct form so LoadAggregateAsync replays correctly.
// They accept both the raw primitive and the legacy {"Value": …} envelope
// shape to stay tolerant of future serializer changes.

internal sealed class ProcessIdJsonConverter : JsonConverter<ProcessId>
{
    public override ProcessId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new ProcessId(reader.GetGuid());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new ProcessId(prop.Value.GetGuid());
        }
        throw new JsonException("Expected Guid string or { Value: Guid } object for ProcessId.");
    }

    public override void Write(Utf8JsonWriter writer, ProcessId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class SourceReferenceJsonConverter : JsonConverter<SourceReference>
{
    public override SourceReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new SourceReference(reader.GetGuid());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new SourceReference(prop.Value.GetGuid());
        }
        throw new JsonException("Expected Guid string or { Value: Guid } object for SourceReference.");
    }

    public override void Write(Utf8JsonWriter writer, SourceReference value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class DiscrepancyIdJsonConverter : JsonConverter<DiscrepancyId>
{
    public override DiscrepancyId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new DiscrepancyId(reader.GetGuid());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new DiscrepancyId(prop.Value.GetGuid());
        }
        throw new JsonException("Expected Guid string or { Value: Guid } object for DiscrepancyId.");
    }

    public override void Write(Utf8JsonWriter writer, DiscrepancyId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class ProcessReferenceJsonConverter : JsonConverter<ProcessReference>
{
    public override ProcessReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new ProcessReference(reader.GetGuid());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new ProcessReference(prop.Value.GetGuid());
        }
        throw new JsonException("Expected Guid string or { Value: Guid } object for ProcessReference.");
    }

    public override void Write(Utf8JsonWriter writer, ProcessReference value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

internal sealed class AmountJsonConverter : JsonConverter<Amount>
{
    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return new Amount(reader.GetDecimal());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new Amount(prop.Value.GetDecimal());
        }
        throw new JsonException("Expected decimal number or { Value: decimal } object for Amount.");
    }

    public override void Write(Utf8JsonWriter writer, Amount value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

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

/// <summary>
/// Reads a <see cref="Currency"/> from either a raw ISO-4217 string or a
/// {"Code":"..."} envelope. Required because the event store persists the
/// payload-mapped form (raw string Code) while DeserializeStored targets the
/// domain event types which declare BaseCurrency/QuoteCurrency/Currency as
/// the Currency value object (record struct { string Code }). Mirrors the
/// canonical pattern of the other stored-options converters.
/// </summary>
internal sealed class CurrencyJsonConverter : JsonConverter<Currency>
{
    public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new Currency(reader.GetString() ?? string.Empty);

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "code", StringComparison.OrdinalIgnoreCase))
                    return new Currency(prop.Value.GetString() ?? string.Empty);
            }
        }

        throw new JsonException("Expected string or { Code: string } object for Currency.");
    }

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Code);
}

/// <summary>
/// Reads a <see cref="Timestamp"/> from either a raw ISO-8601 datetime string or a
/// {"Value":"..."} envelope. Required because the event store persists the
/// payload-mapped form (raw DateTimeOffset) while DeserializeStored targets the
/// domain event types which declare CreatedAt/ActivatedAt/ExpiredAt/EffectiveAt
/// as the Timestamp value object. Absent this converter, multi-event aggregate
/// replay throws JsonException for every domain event that carries a Timestamp
/// field (observed during economic-system/exchange live infrastructure
/// validation, 2026-04-15).
/// </summary>
internal sealed class TimestampJsonConverter : JsonConverter<Timestamp>
{
    public override Timestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new Timestamp(reader.GetDateTimeOffset());

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new Timestamp(prop.Value.GetDateTimeOffset());
            }
        }

        throw new JsonException("Expected ISO-8601 datetime string or { Value: datetime } object for Timestamp.");
    }

    public override void Write(Utf8JsonWriter writer, Timestamp value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

/// <summary>
/// D10: bridge raw Guid string ↔ <see cref="SubjectId"/> wrapper struct so
/// the schema-mapped JSONB ({"OwnerSubjectId":"<guid>"}) round-trips into the
/// domain event's typed value object on replay.
/// </summary>
internal sealed class SubjectIdJsonConverter : JsonConverter<SubjectId>
{
    public override SubjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new SubjectId(reader.GetGuid());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new SubjectId(prop.Value.GetGuid());
        }
        throw new JsonException("Expected Guid string or { Value: Guid } object for SubjectId.");
    }

    public override void Write(Utf8JsonWriter writer, SubjectId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

/// <summary>
/// D10: bridge raw Guid string ↔ <see cref="VaultAccountId"/> wrapper struct.
/// </summary>
internal sealed class VaultAccountIdJsonConverter : JsonConverter<VaultAccountId>
{
    public override VaultAccountId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new VaultAccountId(reader.GetGuid());
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "value", StringComparison.OrdinalIgnoreCase))
                    return new VaultAccountId(prop.Value.GetGuid());
        }
        throw new JsonException("Expected Guid string or { Value: Guid } object for VaultAccountId.");
    }

    public override void Write(Utf8JsonWriter writer, VaultAccountId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

/// <summary>
/// β #2 / INV-REPLAY-LOSSLESS-VALUEOBJECT-01 — generic factory for the
/// canonical "wrapper struct around a single primitive" value-object shape
/// used across the domain (e.g. <c>readonly record struct PathId(Guid Value)</c>,
/// <c>readonly record struct Currency(string Code)</c>). Replaces the per-type
/// converter explosion that would otherwise be required to satisfy the D10
/// invariant for every <c>*Id</c> value object in the codebase.
///
/// Acceptance criteria for a value object to be factory-covered:
///   1. Type is a value type whose namespace starts with <c>Whycespace.Domain.</c>.
///   2. Type exposes a single canonical property named <c>Value</c> or
///      <c>Code</c> whose type is one of the primitive types we handle.
///   3. Type has a constructor accepting that primitive (the canonical
///      validating constructor on every wrapper struct in this codebase).
///
/// On read: accepts the bare primitive form (the schema-mapped JSONB shape)
/// AND the legacy <c>{"Value":...}</c> envelope shape (for back-compat with
/// older writes). On write: emits the bare primitive.
/// </summary>
internal sealed class WrappedPrimitiveValueObjectConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsValueType) return false;
        if (typeToConvert.Namespace?.StartsWith("Whycespace.Domain.", StringComparison.Ordinal) != true) return false;
        return ResolveSinglePrimitiveProperty(typeToConvert) is not null;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var prop = ResolveSinglePrimitiveProperty(typeToConvert)!;
        var converterType = typeof(WrappedPrimitiveValueObjectConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType, prop.PropertyType, prop.Name);
    }

    /// <summary>
    /// Returns the wrapper struct's single primitive property. Tries the
    /// canonical names <c>Value</c> and <c>Code</c> first; otherwise falls
    /// through to ANY single primitive property — covers wrappers that use
    /// semantic names like <c>DurationInDays</c>. Returns null when the
    /// type has zero or multiple primitive properties (multi-property value
    /// objects use STJ's native constructor-binding path, no converter
    /// required).
    /// </summary>
    private static System.Reflection.PropertyInfo? ResolveSinglePrimitiveProperty(Type t)
    {
        bool IsPrim(Type x) => x == typeof(Guid) || x == typeof(string) || x == typeof(decimal)
            || x == typeof(int) || x == typeof(long) || x == typeof(DateTimeOffset);

        var canonical = t.GetProperty("Value") ?? t.GetProperty("Code");
        if (canonical is not null && IsPrim(canonical.PropertyType)) return canonical;

        var props = t.GetProperties().Where(p => p.CanRead && IsPrim(p.PropertyType)).ToArray();
        return props.Length == 1 ? props[0] : null;
    }

    private sealed class WrappedPrimitiveValueObjectConverter<T> : JsonConverter<T> where T : struct
    {
        private readonly Type _primitive;
        private readonly string _propName;

        public WrappedPrimitiveValueObjectConverter(Type primitive, string propName)
        {
            _primitive = primitive;
            _propName = propName;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            object? primitive = ReadPrimitive(ref reader, _primitive, _propName);
            if (primitive is null)
                throw new JsonException($"Could not read primitive for {typeToConvert.Name}.");
            return (T)Activator.CreateInstance(typeToConvert, primitive)!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var prop = typeof(T).GetProperty(_propName)!;
            var raw = prop.GetValue(value);
            switch (raw)
            {
                case Guid g: writer.WriteStringValue(g); break;
                case string s: writer.WriteStringValue(s); break;
                case decimal d: writer.WriteNumberValue(d); break;
                case int i: writer.WriteNumberValue(i); break;
                case long l: writer.WriteNumberValue(l); break;
                case DateTimeOffset dt: writer.WriteStringValue(dt); break;
                default: throw new JsonException($"Unsupported wrapper primitive {raw?.GetType().Name}.");
            }
        }

        private static object? ReadPrimitive(ref Utf8JsonReader reader, Type primitive, string propName)
        {
            // Accept bare primitive (schema-mapped form).
            if (reader.TokenType is JsonTokenType.String or JsonTokenType.Number)
            {
                if (primitive == typeof(Guid)) return reader.GetGuid();
                if (primitive == typeof(string)) return reader.GetString();
                if (primitive == typeof(decimal)) return reader.GetDecimal();
                if (primitive == typeof(int)) return reader.GetInt32();
                if (primitive == typeof(long)) return reader.GetInt64();
                if (primitive == typeof(DateTimeOffset)) return reader.GetDateTimeOffset();
            }
            // Accept {"Value":...} / {"Code":...} legacy envelope form.
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    if (!string.Equals(prop.Name, propName, StringComparison.OrdinalIgnoreCase)) continue;
                    if (primitive == typeof(Guid)) return prop.Value.GetGuid();
                    if (primitive == typeof(string)) return prop.Value.GetString();
                    if (primitive == typeof(decimal)) return prop.Value.GetDecimal();
                    if (primitive == typeof(int)) return prop.Value.GetInt32();
                    if (primitive == typeof(long)) return prop.Value.GetInt64();
                    if (primitive == typeof(DateTimeOffset)) return prop.Value.GetDateTimeOffset();
                }
            }
            return null;
        }
    }
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
            // Generic factory FIRST — handles every Whycespace.Domain.*
            // wrapper struct around a single primitive (Guid Value / string
            // Code / decimal Value / etc.). The per-type converters below are
            // retained because they're already proven-correct for the legacy
            // envelope shape; a duplicate factory match yields the same value.
            new WrappedPrimitiveValueObjectConverterFactory(),
            new AggregateIdJsonConverter(),
            new KanbanListIdJsonConverter(),
            new KanbanCardIdJsonConverter(),
            new KanbanPositionJsonConverter(),
            new TimestampJsonConverter(),
            new CurrencyJsonConverter(),
            new ProcessIdJsonConverter(),
            new SourceReferenceJsonConverter(),
            new DiscrepancyIdJsonConverter(),
            new ProcessReferenceJsonConverter(),
            new AmountJsonConverter(),
            new SubjectIdJsonConverter(),
            new VaultAccountIdJsonConverter(),
            new JsonStringEnumConverter()
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
