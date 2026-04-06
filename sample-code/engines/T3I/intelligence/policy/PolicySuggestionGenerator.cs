using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Policy;

/// <summary>
/// T3I policy suggestion generator. Transforms violation analysis
/// into actionable governance suggestions. Stateless, deterministic.
/// Outputs suggestions — never mutates domain directly.
/// Suggestions flow through governance workflow for human approval.
/// </summary>
public sealed class PolicySuggestionGenerator : IEngine<PolicySuggestionCommand>
{
    public Task<EngineResult> ExecuteAsync(
        PolicySuggestionCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var suggestions = new List<PolicySuggestion>();

        if (command.ViolationRate > 10m)
        {
            suggestions.Add(new PolicySuggestion
            {
                SuggestionType = "ThresholdAdjustment",
                Description = $"Policy '{command.PolicyId}' has violation rate {command.ViolationRate:F1}/hr. Consider raising threshold.",
                Confidence = ConfidenceScore.High,
                Impact = "Reduces false positive violations without weakening security",
                RequiresGuardianApproval = true
            });
        }

        if (command.IsEscalating)
        {
            suggestions.Add(new PolicySuggestion
            {
                SuggestionType = "AdditionalControl",
                Description = $"Escalating violations on policy '{command.PolicyId}'. Consider adding supplementary control.",
                Confidence = ConfidenceScore.Medium,
                Impact = "Adds defense-in-depth layer for escalating risk",
                RequiresGuardianApproval = true
            });
        }

        if (command.CrossRegionAffected)
        {
            suggestions.Add(new PolicySuggestion
            {
                SuggestionType = "JurisdictionOverlay",
                Description = $"Cross-region violations detected for policy '{command.PolicyId}'. Consider jurisdiction-specific overlay.",
                Confidence = ConfidenceScore.Medium,
                Impact = "Region-specific policy tuning without global impact",
                RequiresGuardianApproval = true
            });
        }

        return Task.FromResult(EngineResult.Ok(new PolicySuggestionResult
        {
            PolicyId = command.PolicyId,
            Suggestions = suggestions,
            RequiresGovernanceWorkflow = suggestions.Any(s => s.RequiresGuardianApproval)
        }));
    }
}

public sealed record PolicySuggestionCommand
{
    public required string PolicyId { get; init; }
    public required decimal ViolationRate { get; init; }
    public required bool IsEscalating { get; init; }
    public required bool CrossRegionAffected { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record PolicySuggestion
{
    public required string SuggestionType { get; init; }
    public required string Description { get; init; }
    public required ConfidenceScore Confidence { get; init; }
    public required string Impact { get; init; }
    public required bool RequiresGuardianApproval { get; init; }
}

public sealed record PolicySuggestionResult
{
    public required string PolicyId { get; init; }
    public required IReadOnlyList<PolicySuggestion> Suggestions { get; init; }
    public required bool RequiresGovernanceWorkflow { get; init; }
}
