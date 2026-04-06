using System.Text.Json;
using System.Text.Json.Serialization;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Deterministic event serializer. Produces stable, reproducible JSON output.
/// Used for hash computation, outbox persistence, and replay verification.
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(object payload) =>
        JsonSerializer.Serialize(payload, Options);

    public static string Serialize(EventEnvelope envelope) =>
        JsonSerializer.Serialize(envelope, Options);

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options);
}
