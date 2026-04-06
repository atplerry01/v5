using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Whycespace.Runtime.GuardExecution.Engine;

public sealed record GuardResult
{
    public required string GuardName { get; init; }
    public required bool Passed { get; init; }
    public IReadOnlyList<GuardViolation> Violations { get; init; } = [];
    public DateTimeOffset EvaluatedAt { get; init; }
    public string GuardHash => ComputeHash();

    public bool HasBlockingViolations =>
        Violations.Any(v => v.Severity is GuardSeverity.S0 or GuardSeverity.S1);

    public static GuardResult Pass(string guardName) =>
        new() { GuardName = guardName, Passed = true };

    public static GuardResult Fail(string guardName, IReadOnlyList<GuardViolation> violations) =>
        new() { GuardName = guardName, Passed = false, Violations = violations };

    private string ComputeHash()
    {
        var payload = JsonSerializer.Serialize(new { GuardName, Passed, Violations });
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }
}
