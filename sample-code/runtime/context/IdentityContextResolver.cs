using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Identity;
using Whycespace.Shared.Utils;

namespace Whycespace.Runtime.Context;

/// <summary>
/// Resolves IIdentityContext from CommandContext properties.
/// Reads identity claims enriched by IdentityContextMiddleware.
/// Normalizes trust scores for deterministic hashing.
/// </summary>
public sealed class IdentityContextResolver
{
    /// <summary>
    /// Resolves identity context from the command context.
    /// Returns a populated IIdentityContext or a default anonymous context.
    /// </summary>
    public IIdentityContext Resolve(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var identityId = context.Get<string>(IdentityContextKeys.IdentityId);
        var identityStatus = context.Get<string>(IdentityContextKeys.IdentityStatus);
        var sessionId = context.Get<string>(IdentityContextKeys.SessionId);
        var deviceId = context.Get<string>(IdentityContextKeys.DeviceId);
        var trustLevel = context.Get<string>(IdentityContextKeys.TrustLevel);

        var subjectId = identityId ?? "anonymous";

        // Parse trust score from string, normalize for determinism
        var trustScore = 0.0;
        if (trustLevel is not null && double.TryParse(trustLevel,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            trustScore = TrustScoreNormalizer.Normalize(parsed);
        }

        // Parse roles from comma-separated string
        var rolesRaw = context.Get<string>(IdentityContextKeys.Roles);
        var roles = !string.IsNullOrWhiteSpace(rolesRaw)
            ? rolesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];

        // Verified = has a non-null identity with status
        var isAuthenticated = !string.IsNullOrEmpty(identityId);
        var isVerified = isAuthenticated
            && identityStatus is "verified" or "active";

        return new ResolvedIdentityContext
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            DeviceId = deviceId,
            Roles = roles,
            TrustScore = trustScore,
            IsVerified = isVerified
        };
    }
}

/// <summary>
/// Concrete implementation of IIdentityContext resolved from runtime context.
/// </summary>
public sealed record ResolvedIdentityContext : IIdentityContext
{
    public required string SubjectId { get; init; }
    public string? SessionId { get; init; }
    public string? DeviceId { get; init; }
    public required string[] Roles { get; init; }
    public required double TrustScore { get; init; }
    public required bool IsVerified { get; init; }
}
