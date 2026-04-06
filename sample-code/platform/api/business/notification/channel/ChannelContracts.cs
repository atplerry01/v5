namespace Whycespace.Platform.Api.Business.Notification.Channel;

public sealed record ChannelRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ChannelResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
