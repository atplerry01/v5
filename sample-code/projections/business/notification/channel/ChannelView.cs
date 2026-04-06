namespace Whycespace.Projections.Business.Notification.Channel;

public sealed record ChannelView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
