namespace Whycespace.Engines.T3I.Simulation.Governance;

/// <summary>
/// Generates structured governance proposals from loop results.
/// Proposals are submitted to the domain governance BC for approval via vote/ballot.
/// This engine is pure computation — no persistence, no side effects.
/// </summary>
public sealed class GovernanceProposalGenerator
{
    public IReadOnlyList<StructuredProposal> Generate(GovernanceLoopResult loopResult, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(loopResult);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        if (loopResult.Proposals.Count == 0)
            return [];

        var proposals = new List<StructuredProposal>();

        foreach (var proposal in loopResult.Proposals)
        {
            var requiredApprovers = proposal.Priority switch
            {
                "Critical" => 3,
                "High" => 2,
                _ => 1
            };

            var expirationHours = proposal.Priority switch
            {
                "Critical" => 4,
                "High" => 24,
                _ => 72
            };

            proposals.Add(new StructuredProposal(
                ProposalType: $"AUTO_{proposal.DetectionType}",
                Title: $"[Auto] {proposal.Summary}",
                Description: proposal.RecommendedAction,
                Priority: proposal.Priority,
                Confidence: proposal.SimulationConfidence,
                RequiredApprovers: requiredApprovers,
                ExpirationHours: expirationHours,
                CorrelationId: correlationId,
                RequiresQuorum: proposal.RequiresQuorum));
        }

        return proposals;
    }
}

public sealed record StructuredProposal(
    string ProposalType,
    string Title,
    string Description,
    string Priority,
    decimal Confidence,
    int RequiredApprovers,
    int ExpirationHours,
    string CorrelationId,
    bool RequiresQuorum);
