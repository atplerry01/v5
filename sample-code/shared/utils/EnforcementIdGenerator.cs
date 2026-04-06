using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Utils;

/// <summary>
/// Deterministic ID generator for EnforcementActions.
/// Same violation + same target → same ID → prevents duplicate enforcement.
/// Replay-safe: replaying a violation does NOT generate a new action ID.
/// </summary>
public static class EnforcementIdGenerator
{
    public static Guid Generate(
        string correlationId,
        string targetId,
        string enforcementType,
        string violationReason)
    {
        var input = $"enforcement:{correlationId}:{targetId}:{enforcementType}:{violationReason}";
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
