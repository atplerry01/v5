using System.Text.Json;

namespace Whycespace.Platform.Api.Core.Contracts;

/// <summary>
/// Well-known header keys for carrying WhyceIdentity through the platform pipeline.
/// Mirrors the runtime IdentityContextKeys pattern but scoped to the platform layer.
/// IdentityMiddleware writes these; downstream middleware and controller read them.
/// </summary>
public static class IdentityHeaderKeys
{
    public const string IdentityId = "X-WhyceId-IdentityId";
    public const string Roles = "X-WhyceId-Roles";
    public const string Attributes = "X-WhyceId-Attributes";
    public const string TrustScore = "X-WhyceId-TrustScore";
    public const string Consents = "X-WhyceId-Consents";
    public const string IsVerified = "X-WhyceId-IsVerified";
    public const string SessionId = "X-WhyceId-SessionId";

    /// <summary>
    /// Writes WhyceIdentity fields into the headers dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Enrich(
        IReadOnlyDictionary<string, string> existing, WhyceIdentity identity)
    {
        var headers = new Dictionary<string, string>(existing)
        {
            [IdentityId] = identity.IdentityId.ToString(),
            [Roles] = string.Join(",", identity.Roles),
            [TrustScore] = identity.TrustScore.ToString("F4"),
            [Consents] = string.Join(",", identity.Consents),
            [IsVerified] = identity.IsVerified.ToString(),
            [Attributes] = JsonSerializer.Serialize(identity.Attributes)
        };

        if (identity.SessionId is not null)
            headers[SessionId] = identity.SessionId;

        return headers;
    }

    /// <summary>
    /// Extracts WhyceIdentity from headers written by IdentityMiddleware.
    /// Returns null if identity headers are not present.
    /// </summary>
    public static WhyceIdentity? Extract(IReadOnlyDictionary<string, string> headers)
    {
        if (!headers.TryGetValue(IdentityId, out var idStr) || !Guid.TryParse(idStr, out var identityId))
            return null;

        var roles = headers.TryGetValue(Roles, out var rolesStr) && !string.IsNullOrEmpty(rolesStr)
            ? rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : new List<string>();

        var trustScore = headers.TryGetValue(TrustScore, out var trustStr) && decimal.TryParse(trustStr, out var ts)
            ? ts
            : 0m;

        var consents = headers.TryGetValue(Consents, out var consentsStr) && !string.IsNullOrEmpty(consentsStr)
            ? consentsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : new List<string>();

        var isVerified = headers.TryGetValue(IsVerified, out var verifiedStr)
            && bool.TryParse(verifiedStr, out var v) && v;

        var attributes = headers.TryGetValue(Attributes, out var attrStr) && !string.IsNullOrEmpty(attrStr)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(attrStr) ?? new Dictionary<string, string>()
            : new Dictionary<string, string>();

        headers.TryGetValue(SessionId, out var sessionId);

        return new WhyceIdentity
        {
            IdentityId = identityId,
            Roles = roles,
            Attributes = attributes,
            TrustScore = trustScore,
            Consents = consents,
            IsVerified = isVerified,
            SessionId = sessionId
        };
    }
}
