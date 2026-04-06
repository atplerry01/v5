using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Generates governance-ready proposal drafts from recommendations.
/// Output is advisory-only — human approval required before activation.
/// </summary>
public sealed class PolicyDraftGenerator
{
    private readonly IClock _clock;

    public PolicyDraftGenerator(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public IReadOnlyList<GovernanceProposalDraft> GenerateDrafts(GovernanceRecommendationAggregate recommendation)
    {
        var drafts = new List<GovernanceProposalDraft>();

        foreach (var policyId in recommendation.AffectedPolicies)
        {
            var evidence = recommendation.Insights
                .Select(i => $"[{i.Type}:{i.Severity}] {i.Description}")
                .ToList();

            var versionDiff = BuildVersionDiff(recommendation, policyId);

            drafts.Add(new GovernanceProposalDraft
            {
                ProposalId = DeterministicIdHelper.FromSeed($"ProposalDraft:{recommendation.RecommendationId.Value}:{policyId}"),
                RecommendationId = recommendation.RecommendationId.Value,
                PolicyId = policyId,
                VersionDiff = versionDiff,
                Rationale = BuildRationale(recommendation),
                ExpectedImpact = recommendation.Impact.CompositeScore,
                Confidence = recommendation.Confidence.Value,
                Risk = recommendation.Risk.Score,
                SupportingEvidence = evidence,
                GeneratedAt = _clock.UtcNowOffset
            });
        }

        return drafts;
    }

    private static string BuildVersionDiff(GovernanceRecommendationAggregate recommendation, Guid policyId)
    {
        var changes = string.Join("; ", recommendation.SuggestedChanges);
        return $"[Policy:{policyId}] Suggested: {changes}";
    }

    private static string BuildRationale(GovernanceRecommendationAggregate recommendation)
    {
        var insightSummary = recommendation.Insights.Count == 1
            ? "1 insight"
            : $"{recommendation.Insights.Count} insights";

        return $"Recommendation from {recommendation.Source} analysis. " +
               $"Based on {insightSummary} with confidence {recommendation.Confidence.Value:F2}. " +
               $"Risk: {recommendation.Risk.Category} ({recommendation.Risk.Score}/100).";
    }
}
