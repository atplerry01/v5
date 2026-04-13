using System.Security.Cryptography;
using System.Text;
using Whycespace.Engines.T0U.WhyceId.Model;

namespace Whycespace.Engines.T0U.WhyceId.Resolver;

/// <summary>
/// Resolves an identity from token or userId input.
/// Deterministic: same input always produces same IdentityId.
/// </summary>
public static class IdentityResolver
{
    public static string ResolveIdentityId(string? token, string? userId)
    {
        if (userId is not null)
            return userId;

        if (token is not null)
            return HashToken(token);

        return string.Empty;
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }

    public static string ComputeDeviceHash(string deviceId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"device:{deviceId}"));
        return Convert.ToHexStringLower(bytes);
    }
}
