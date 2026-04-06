namespace Whycespace.Engines.T0U.Governance.IdentityGovernance;

/// <summary>
/// Consent gate — checks if required consent is present before allowing identity actions.
/// Stateless — no domain imports, works with primitive types.
/// ALL actions MUST pass policy; deny = STOP execution.
/// </summary>
public sealed class IdentityConsentGate
{
    public ConsentGateResult Evaluate(ConsentGateCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!IdentityGovernanceWorkflows.RequiresConsent(command.Action))
            return ConsentGateResult.NotRequired();

        if (!command.HasActiveConsent)
            return ConsentGateResult.Denied($"Action '{command.Action}' requires active consent from identity '{command.IdentityId}'.");

        if (command.ConsentExpired)
            return ConsentGateResult.Denied($"Consent for action '{command.Action}' has expired.");

        return ConsentGateResult.Allowed();
    }
}

public sealed record ConsentGateCommand(
    string IdentityId,
    string Action,
    bool HasActiveConsent,
    bool ConsentExpired);

public sealed record ConsentGateResult(
    bool IsAllowed,
    bool IsRequired,
    string? Reason = null)
{
    public static ConsentGateResult Allowed() => new(true, true);
    public static ConsentGateResult NotRequired() => new(true, false);
    public static ConsentGateResult Denied(string reason) => new(false, true, reason);
}
