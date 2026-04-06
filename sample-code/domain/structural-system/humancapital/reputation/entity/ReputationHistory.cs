namespace Whycespace.Domain.StructuralSystem.HumanCapital.Reputation;

public sealed class ReputationHistory
{
    public Guid Id { get; init; }
    public DateTimeOffset RecordedAt { get; init; }
    public double PreviousScore { get; init; }
    public double NewScore { get; init; }
}
