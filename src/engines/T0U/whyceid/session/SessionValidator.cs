using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Engines.T0U.WhyceId.Session;

/// <summary>
/// Validates sessions deterministically.
/// Session IDs are derived from identity + device context.
/// </summary>
public static class SessionValidator
{
    public static string GenerateSessionId(string identityId, string? deviceId)
    {
        var seed = deviceId is not null
            ? $"session:{identityId}:{deviceId}"
            : $"session:{identityId}";

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(bytes);
    }

    public static bool ValidateSession(string sessionId, string identityId, string? deviceId)
    {
        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(identityId))
            return false;

        var expectedSessionId = GenerateSessionId(identityId, deviceId);
        return string.Equals(sessionId, expectedSessionId, StringComparison.Ordinal);
    }
}
