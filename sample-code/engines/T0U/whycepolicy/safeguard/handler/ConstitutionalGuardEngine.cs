namespace Whycespace.Engines.T0U.WhycePolicy.Safeguard;

/// <summary>
/// Constitutional safeguard: blocks unauthorized policy changes,
/// privilege escalation, illegal activation, and governance bypass.
/// Runs BEFORE governance engine.
/// </summary>
public sealed class ConstitutionalGuardEngine
{
    private static readonly HashSet<string> ProtectedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "policy.create", "policy.update", "policy.deprecate", "policy.activate",
        "governance.override", "governance.quorum.modify",
        "system.config.modify", "economic.rule.modify"
    };

    public ConstitutionalGuardResult Validate(
        Guid actorId,
        string action,
        bool hasValidProposal,
        bool isQuorumMet)
    {
        // Block: empty actor
        if (actorId == Guid.Empty)
            return ConstitutionalGuardResult.Denied("Anonymous actors cannot modify policies.");

        // Block: protected actions without governance
        if (ProtectedActions.Contains(action))
        {
            if (!hasValidProposal)
                return ConstitutionalGuardResult.Denied(
                    $"Action '{action}' requires an approved governance proposal.");

            if (!isQuorumMet)
                return ConstitutionalGuardResult.Denied(
                    $"Action '{action}' requires quorum approval before execution.");
        }

        return ConstitutionalGuardResult.Allowed();
    }
}

public sealed record ConstitutionalGuardResult
{
    public required bool IsAllowed { get; init; }
    public string? Reason { get; init; }

    public bool IsDenied => !IsAllowed;

    public static ConstitutionalGuardResult Allowed() => new() { IsAllowed = true };
    public static ConstitutionalGuardResult Denied(string reason) => new() { IsAllowed = false, Reason = reason };
}
