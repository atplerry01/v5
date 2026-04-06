using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Participant;

public sealed class ParticipantProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.participant";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.participant.created",
        "whyce.structural.humancapital.participant.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IParticipantViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ParticipantReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
