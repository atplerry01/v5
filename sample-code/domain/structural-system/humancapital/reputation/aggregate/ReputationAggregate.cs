using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Reputation;

public sealed class ReputationAggregate : AggregateRoot
{
    public ReputationScore Score { get; private set; } = new(0);
    public ReputationHistory? History { get; private set; }

    public static ReputationAggregate Create(Guid reputationId, double initialScore)
    {
        var reputation = new ReputationAggregate();
        reputation.Apply(new ReputationUpdatedEvent(reputationId, initialScore));
        return reputation;
    }

    public void Update(Guid historyId, double newScore, DateTimeOffset recordedAt)
    {
        var previousScore = Score.Value;
        Apply(new ReputationUpdatedEvent(Id, newScore));
        History = new ReputationHistory
        {
            Id = historyId,
            RecordedAt = recordedAt,
            PreviousScore = previousScore,
            NewScore = newScore
        };
    }

    private void Apply(ReputationUpdatedEvent e)
    {
        Id = e.ReputationId;
        Score = new ReputationScore(e.NewScore);
        RaiseDomainEvent(e);
    }
}
