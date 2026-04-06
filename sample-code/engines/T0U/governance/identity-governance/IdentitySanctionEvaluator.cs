namespace Whycespace.Engines.T0U.Governance.IdentityGovernance;

/// <summary>
/// Evaluates whether an identity action should trigger a sanction.
/// Stateless decision engine — no domain imports.
/// </summary>
public sealed class IdentitySanctionEvaluator
{
    public SanctionEvaluationResult Evaluate(SanctionEvaluationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!IdentityGovernanceWorkflows.TriggersSanction(command.Action))
            return SanctionEvaluationResult.NoSanction();

        var severity = command.Action switch
        {
            "identity.suspend" => "High",
            "credential.revoke" => "High",
            "device.block" => "Medium",
            "session.revoke" => "Low",
            _ => "Low"
        };

        return SanctionEvaluationResult.Sanction(severity, $"Action '{command.Action}' on target '{command.TargetId}' triggers {severity} severity sanction.");
    }
}

public sealed record SanctionEvaluationCommand(
    string ActorId,
    string Action,
    string TargetId,
    string Reason);

public sealed record SanctionEvaluationResult(
    bool RequiresSanction,
    string Severity,
    string? Description = null)
{
    public static SanctionEvaluationResult NoSanction() => new(false, "None");
    public static SanctionEvaluationResult Sanction(string severity, string description) => new(true, severity, description);
}
