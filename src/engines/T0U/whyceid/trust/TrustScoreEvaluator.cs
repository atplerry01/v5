using System.Security.Cryptography;
using System.Text;

namespace Whyce.Engines.T0U.WhyceId.Trust;

/// <summary>
/// Evaluates trust score based on identity attributes.
/// Deterministic: same inputs always produce same score.
/// Score range: 0 (no trust) to 100 (full trust).
/// </summary>
public static class TrustScoreEvaluator
{
    private const int BaseScore = 10;
    private const int VerifiedBonus = 30;
    private const int RoleBonus = 5;
    private const int DeviceBonus = 10;
    private const int MaxScore = 100;

    public static (int Score, string[] Factors) Evaluate(
        string identityId,
        string[] roles,
        string? deviceId,
        bool isVerified)
    {
        var factors = new List<string>();
        var score = BaseScore;
        factors.Add($"base:{BaseScore}");

        if (isVerified)
        {
            score += VerifiedBonus;
            factors.Add($"verified:{VerifiedBonus}");
        }

        var roleScore = Math.Min(roles.Length * RoleBonus, 25);
        if (roleScore > 0)
        {
            score += roleScore;
            factors.Add($"roles:{roleScore}");
        }

        if (deviceId is not null)
        {
            score += DeviceBonus;
            factors.Add($"device:{DeviceBonus}");
        }

        score = Math.Min(score, MaxScore);
        return (score, factors.ToArray());
    }

    public static string ComputeTrustHash(string identityId, int score, string[] factors)
    {
        var sortedFactors = string.Join(",", factors.OrderBy(f => f, StringComparer.Ordinal));
        var input = $"{identityId}:{score}:{sortedFactors}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
