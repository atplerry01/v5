namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;

public sealed record ChannelReadModel
{
    public Guid ChannelId { get; init; }
    public Guid StreamId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
