using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;

public sealed class AutonomousDecisionService
{
    public AutonomousDecision? Decide(
        IEnumerable<AutonomousCandidate> candidates,
        AutonomyConstraints constraints)
    {
        var filtered = candidates
            .Where(x => x.EstimatedCost <= constraints.MaxCost)
            .Where(x => x.Path.All(constraints.AllowedEntities.Contains))
            .OrderByDescending(x => x.Profit)
            .ToList();

        if (filtered.Count == 0)
            return null;

        var selected = filtered[0];

        var hashSeed = string.Join(":",
            string.Join(",", selected.Path),
            selected.EstimatedCost,
            selected.EstimatedRevenue);

        var decisionId = DeterministicIdHelper.FromSeed($"autonomy:decision:{hashSeed}");
        var decisionHash = DeterministicIdHelper.FromSeed($"autonomy:hash:{hashSeed}").ToString("N");

        return new AutonomousDecision(
            decisionId,
            selected.Path,
            decisionHash);
    }
}
