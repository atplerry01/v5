using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentTimelineEntry
{
    public Guid Id { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public Guid? ActorIdentityId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private IncidentTimelineEntry() { }

    public static IncidentTimelineEntry Record(string action, string detail, DateTimeOffset timestamp, Guid? actorIdentityId = null)
    {
        return new IncidentTimelineEntry
        {
            Id = DeterministicIdHelper.FromSeed($"IncidentTimelineEntry:{action}:{detail}:{actorIdentityId}"),
            Action = action,
            Detail = detail,
            ActorIdentityId = actorIdentityId,
            OccurredAt = timestamp
        };
    }
}
