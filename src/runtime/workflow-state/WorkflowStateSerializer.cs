using System.Text.Json;

namespace Whyce.Runtime.WorkflowState;

public static class WorkflowStateSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(Dictionary<string, string> state)
    {
        var sorted = new SortedDictionary<string, string>(state, StringComparer.Ordinal);
        return JsonSerializer.Serialize(sorted, Options);
    }

    public static Dictionary<string, string> Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, Options)
               ?? new Dictionary<string, string>();
    }
}
