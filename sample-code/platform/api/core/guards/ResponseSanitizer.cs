using System.Text.Json;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Guards;

/// <summary>
/// E19.17.5 — Response Sanitization Guard.
/// Strips sensitive internal data from all platform responses before return.
///
/// REMOVED:
/// - Internal IDs (aggregate IDs, stream IDs, snapshot IDs)
/// - Policy internals (rule chains, decision trees, enforcement details)
/// - Raw event payloads (domain events, command payloads)
/// - Engine references (engine names, tier identifiers)
/// - Stack traces (handled by ErrorHandlingMiddleware, but double-checked here)
///
/// Applied globally as the LAST step before response return.
/// </summary>
public static class ResponseSanitizer
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        // Internal IDs
        "aggregateId",
        "streamId",
        "snapshotId",
        "partitionKey",
        "sequenceNumber",
        "eventId",
        "commandId",
        "sagaId",
        "outboxId",

        // Policy internals
        "policyRuleChain",
        "decisionTree",
        "enforcementDetail",
        "policyBindings",
        "opaInput",
        "opaOutput",
        "regoResult",

        // Engine references
        "engineName",
        "engineTier",
        "engineCluster",
        "executionEngine",
        "runtimeContext",

        // Raw payloads
        "rawEvent",
        "rawCommand",
        "rawPayload",
        "internalPayload",

        // Stack traces
        "stackTrace",
        "innerException",
        "exceptionType",
        "targetSite"
    };

    /// <summary>
    /// Sanitizes an ApiResponse by stripping sensitive fields from its Data payload.
    /// Returns a new response with sanitized data.
    /// </summary>
    public static ApiResponse Sanitize(ApiResponse response)
    {
        if (response.Data is null)
            return response;

        var sanitizedData = SanitizeObject(response.Data);

        return response with { Data = sanitizedData };
    }

    /// <summary>
    /// Middleware-style sanitization that wraps the next handler.
    /// </summary>
    public static async Task<ApiResponse> SanitizeAsync(
        ApiRequest request,
        Func<ApiRequest, Task<ApiResponse>> next)
    {
        var response = await next(request);
        return Sanitize(response);
    }

    private static object? SanitizeObject(object? value)
    {
        if (value is null)
            return null;

        if (value is JsonElement jsonElement)
            return SanitizeJsonElement(jsonElement);

        // For strongly-typed objects, serialize to JSON then sanitize
        try
        {
            var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            using var doc = JsonDocument.Parse(json);
            return SanitizeJsonElement(doc.RootElement);
        }
        catch
        {
            // If serialization fails, return the original value — don't leak errors
            return value;
        }
    }

    private static object? SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var sanitized = new Dictionary<string, object?>();
                foreach (var property in element.EnumerateObject())
                {
                    if (SensitiveKeys.Contains(property.Name))
                        continue; // Strip sensitive key

                    sanitized[property.Name] = SanitizeJsonElement(property.Value);
                }
                return sanitized;

            case JsonValueKind.Array:
                var items = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    items.Add(SanitizeJsonElement(item));
                }
                return items;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                return element.GetDecimal();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return null;
        }
    }
}
