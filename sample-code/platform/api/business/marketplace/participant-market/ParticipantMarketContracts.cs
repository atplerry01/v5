namespace Whycespace.Platform.Api.Business.Marketplace.ParticipantMarket;

public sealed record ParticipantMarketRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ParticipantMarketResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
