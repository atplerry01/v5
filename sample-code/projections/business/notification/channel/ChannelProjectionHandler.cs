using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Notification.Channel;

public sealed class ChannelProjectionHandler
{
    public string ProjectionName => "whyce.business.notification.channel";

    public string[] EventTypes =>
    [
        "whyce.business.notification.channel.created",
        "whyce.business.notification.channel.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IChannelViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ChannelReadModel
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
