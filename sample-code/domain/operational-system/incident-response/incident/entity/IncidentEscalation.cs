using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentEscalation
{
    public Guid Id { get; private set; }
    public int FromLevel { get; private set; }
    public int ToLevel { get; private set; }
    public string PreviousSeverity { get; private set; } = string.Empty;
    public string NewSeverity { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset EscalatedAt { get; private set; }

    private IncidentEscalation() { }

    public static IncidentEscalation Create(
        int fromLevel, int toLevel,
        string previousSeverity, string newSeverity,
        string reason, DateTimeOffset timestamp)
    {
        return new IncidentEscalation
        {
            Id = DeterministicIdHelper.FromSeed($"IncidentEscalation:{fromLevel}:{toLevel}:{newSeverity}:{reason}"),
            FromLevel = fromLevel,
            ToLevel = toLevel,
            PreviousSeverity = previousSeverity,
            NewSeverity = newSeverity,
            Reason = reason,
            EscalatedAt = timestamp
        };
    }
}
