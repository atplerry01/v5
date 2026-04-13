namespace Whycespace.Shared.Contracts.Identity;

/// <summary>
/// Contract for identity resolution.
/// Used by runtime middleware to resolve identity from external identity providers.
/// </summary>
public interface IIdentityResolver
{
    Task<IdentityResolution> ResolveAsync(string? token, string? userId);
}

public sealed record IdentityResolution(
    string IdentityId,
    bool IsAuthenticated,
    string[] Roles,
    int TrustScore);
