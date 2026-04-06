namespace Whycespace.Engines.T0U.Governance.IdentityGovernance;

/// <summary>
/// Validates identity governance proposals before submission.
/// Stateless — no persistence, no domain imports.
/// </summary>
public sealed class IdentityProposalValidator
{
    public ProposalValidationResult Validate(IdentityProposalCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.ProposerId))
            return ProposalValidationResult.Invalid("ProposerId is required.");

        if (string.IsNullOrWhiteSpace(command.Action))
            return ProposalValidationResult.Invalid("Action is required.");

        if (!IdentityGovernanceWorkflows.RequiresProposal(command.Action))
            return ProposalValidationResult.Invalid($"Action '{command.Action}' does not require governance proposal.");

        if (string.IsNullOrWhiteSpace(command.Justification))
            return ProposalValidationResult.Invalid("Justification is required for governance proposals.");

        return ProposalValidationResult.Valid();
    }
}

public sealed record IdentityProposalCommand(
    string ProposerId,
    string Action,
    string TargetId,
    string Justification);

public sealed record ProposalValidationResult(bool IsValid, string? Reason = null)
{
    public static ProposalValidationResult Valid() => new(true);
    public static ProposalValidationResult Invalid(string reason) => new(false, reason);
}
