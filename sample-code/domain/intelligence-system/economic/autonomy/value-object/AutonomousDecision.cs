namespace Whycespace.Domain.IntelligenceSystem.Economic.Autonomy;

public sealed class AutonomousDecision
{
    public Guid DecisionId { get; }
    public IReadOnlyCollection<Guid> SelectedPath { get; }
    public string DecisionHash { get; }

    public AutonomousDecision(
        Guid decisionId,
        IEnumerable<Guid> path,
        string hash)
    {
        DecisionId = decisionId;
        SelectedPath = path.Distinct().ToList().AsReadOnly();
        DecisionHash = hash;
    }
}
