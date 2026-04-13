using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Runtime.Context;

/// <summary>
/// Runtime execution context. Enriches the shared CommandContext with
/// runtime-specific metadata resolved during pipeline execution.
/// Provides deterministic execution ID generation for replay support.
/// </summary>
public static class RuntimeExecutionContext
{
    public const string RuntimeOriginKey = "Runtime.IsFromRuntime";
    public const string IdentityIdKey = "Runtime.IdentityId";
    public const string PolicyDecisionHashKey = "Runtime.PolicyDecisionHash";
    public const string TraceIdKey = "Runtime.TraceId";
    public const string PartitionKeyKey = "Runtime.PartitionKey";

    /// <summary>
    /// Generates a deterministic execution ID from command metadata.
    /// Same input always produces the same execution ID, enabling replay verification.
    /// </summary>
    public static string GenerateExecutionId(Guid commandId, string commandType, Guid correlationId, DateTimeOffset timestamp)
    {
        var input = $"{commandId}:{commandType}:{correlationId}:{timestamp:O}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return $"exec-{Convert.ToHexString(hash)[..16].ToLowerInvariant()}";
    }
}
