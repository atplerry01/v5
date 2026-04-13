using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Context;

/// <summary>
/// Resolves identity context from the command context.
/// Extracts identity metadata populated by PolicyMiddleware
/// (WhyceIdEngine identity resolution).
/// </summary>
public sealed class IdentityContextResolver
{
    /// <summary>
    /// Resolves the identity context from a fully-enriched command context.
    /// Returns null if identity has not been resolved (pre-policy stage).
    /// </summary>
    public IdentityContext? Resolve(CommandContext context)
    {
        if (string.IsNullOrWhiteSpace(context.IdentityId))
            return null;

        return new IdentityContext
        {
            IdentityId = context.IdentityId,
            Roles = context.Roles ?? [],
            TrustScore = context.TrustScore ?? 0,
            TenantId = context.TenantId,
            ActorId = context.ActorId
        };
    }
}

public sealed record IdentityContext
{
    public required string IdentityId { get; init; }
    public required string[] Roles { get; init; }
    public required int TrustScore { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
}
