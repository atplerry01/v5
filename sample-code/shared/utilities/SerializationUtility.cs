using System.Text.Json;
using System.Text.Json.Serialization;

namespace Whycespace.Shared.Utilities;

public static class SerializationUtility
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, DefaultOptions);

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public static string SerializeIndented<T>(T value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions(DefaultOptions) { WriteIndented = true });
}
