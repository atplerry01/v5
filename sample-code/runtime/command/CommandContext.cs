using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Command;

public sealed class CommandContext
{
    public required CommandEnvelope Envelope { get; init; }
    public required string ExecutionId { get; init; }
    public required IClock Clock { get; init; }
    public string? TraceId { get; init; }
    public string? PartitionKey { get; init; }
    public Dictionary<string, object> Properties { get; } = new();
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Well-known property key used by ExecutionGuardMiddleware to mark runtime origin.
    /// </summary>
    public const string RuntimeOriginKey = "Runtime.IsFromRuntime";

    public bool IsFromRuntime =>
        Properties.TryGetValue(RuntimeOriginKey, out var value) && value is true;

    public void Set<T>(string key, T value) where T : notnull
    {
        Properties[key] = value;
    }

    public T? Get<T>(string key) where T : class
    {
        return Properties.TryGetValue(key, out var value) ? value as T : null;
    }

    /// <summary>
    /// Generates a deterministic execution ID from a command envelope.
    /// Same envelope always produces the same execution ID, enabling replay.
    /// </summary>
    public static string GenerateExecutionId(CommandEnvelope envelope)
    {
        var input = $"{envelope.CommandId}:{envelope.CommandType}:{envelope.CorrelationId}:{envelope.Timestamp:O}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return $"exec-{Convert.ToHexString(hash)[..16].ToLowerInvariant()}";
    }
}
