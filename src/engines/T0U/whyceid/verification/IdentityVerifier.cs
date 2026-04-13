using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Engines.T0U.WhyceId.Verification;

/// <summary>
/// Verifies identity claims deterministically.
/// </summary>
public static class IdentityVerifier
{
    public static (bool IsVerified, string VerificationHash) Verify(
        string identityId,
        string verificationMethod,
        string verificationPayload)
    {
        if (string.IsNullOrEmpty(identityId) ||
            string.IsNullOrEmpty(verificationMethod) ||
            string.IsNullOrEmpty(verificationPayload))
        {
            return (false, string.Empty);
        }

        var hash = ComputeVerificationHash(identityId, verificationMethod, verificationPayload);
        return (true, hash);
    }

    public static string ComputeVerificationHash(
        string identityId, string method, string payload)
    {
        var input = $"verify:{identityId}:{method}:{payload}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
